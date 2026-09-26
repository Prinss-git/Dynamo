using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IAccessService _accessService;
        private readonly IMapper _mapper;

        public OrganizationService(IOrganizationRepository repository,
                                   IUserRepository userRepository,
                                   IAccessService accessService,
                                   IMapper mapper)
        {
            _repository = repository;
            _userRepository = userRepository;
            _accessService = accessService;
            _mapper = mapper;
        }

        public List<OrganizationViewModel> GetOrganizations(string search)
        {
            var query = _repository.GetOrganizations();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(term)
                                      || (o.Acronym != null && o.Acronym.ToLower().Contains(term)));
            }

            return query.OrderBy(o => o.Name)
                .Select(o => new OrganizationViewModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Acronym = o.Acronym,
                    Description = o.Description,
                    Adviser = o.Adviser,
                    IsActive = o.IsActive,
                    MemberCount = o.Members.Count,
                    EventCount = o.Events.Count,
                })
                .ToList();
        }

        public OrganizationViewModel GetOrganization(int id)
        {
            return _mapper.Map<OrganizationViewModel>(FindOrganization(id));
        }

        public void CreateOrganization(OrganizationViewModel model, string actor)
        {
            EnsureNameAvailable(model.Name, 0);

            var organization = _mapper.Map<Organization>(model);
            organization.Id = 0;
            organization.CreatedTime = DateTime.Now;
            organization.UpdatedTime = DateTime.Now;
            organization.CreatedBy = actor;
            organization.UpdatedBy = actor;

            _repository.AddOrganization(organization);
        }

        public void UpdateOrganization(OrganizationViewModel model, string actor)
        {
            var organization = FindOrganization(model.Id);
            EnsureNameAvailable(model.Name, model.Id);

            _mapper.Map(model, organization);
            organization.UpdatedTime = DateTime.Now;
            organization.UpdatedBy = actor;

            _repository.UpdateOrganization(organization);
        }

        public void DeleteOrganization(int id)
        {
            var organization = FindOrganization(id);
            if (_repository.GetOrganizations().Where(o => o.Id == id).SelectMany(o => o.Events).Any())
            {
                throw new InvalidDataException(Resources.Messages.Errors.OrganizationHasEvents);
            }

            _repository.DeleteOrganization(organization);
        }

        public OrganizationMembersModel GetMembers(int organizationId)
        {
            var model = new OrganizationMembersModel
            {
                Organization = GetOrganization(organizationId),
                Members = _repository.GetMembers()
                    .Where(m => m.OrganizationId == organizationId)
                    .OrderBy(m => m.Member.Name)
                    .Select(m => new OrganizationMemberViewModel
                    {
                        Id = m.Id,
                        OrganizationId = m.OrganizationId,
                        MemberId = m.MemberId,
                        UserId = m.Member.UserId,
                        MemberName = m.Member.Name,
                        StudentNumber = m.Member.StudentNumber,
                        Role = m.Member.Role,
                        Position = m.Position,
                        JoinedTime = m.JoinedTime,
                    })
                    .ToList(),
            };

            var memberIds = model.Members.Select(m => m.MemberId).ToList();
            model.AvailableUsers = _userRepository.GetUsers()
                .Where(u => u.IsActive && !memberIds.Contains(u.Id))
                .OrderBy(u => u.Name)
                .Select(u => new OptionModel
                {
                    Id = u.Id,
                    Text = u.Name + " (" + (u.StudentNumber ?? u.UserId) + ") - " + u.Role,
                })
                .ToList();

            return model;
        }

        public void AddMember(int organizationId, int memberId, string position)
        {
            FindOrganization(organizationId);
            if (!_userRepository.GetUsers().Any(u => u.Id == memberId))
            {
                throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
            }
            if (_repository.GetMembers().Any(m => m.OrganizationId == organizationId && m.MemberId == memberId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.AlreadyMember);
            }

            _repository.AddMember(new OrganizationMember
            {
                OrganizationId = organizationId,
                MemberId = memberId,
                Position = string.IsNullOrWhiteSpace(position) ? "Member" : position.Trim(),
                JoinedTime = DateTime.Now,
            });
        }

        public void UpdateMemberPosition(int membershipId, string position)
        {
            var membership = FindMembership(membershipId);
            membership.Position = string.IsNullOrWhiteSpace(position) ? "Member" : position.Trim();
            _repository.UpdateMember(membership);
        }

        public int RemoveMember(int membershipId)
        {
            var membership = FindMembership(membershipId);
            _repository.DeleteMember(membership);
            return membership.OrganizationId;
        }

        public List<OptionModel> GetManagedOrganizationOptions(CurrentUserModel currentUser)
        {
            var ids = _accessService.GetManagedOrganizationIds(currentUser);
            return _repository.GetOrganizations()
                .Where(o => o.IsActive && ids.Contains(o.Id))
                .OrderBy(o => o.Name)
                .Select(o => new OptionModel { Id = o.Id, Text = o.Name })
                .ToList();
        }

        private Organization FindOrganization(int id)
        {
            return _repository.GetOrganizations().FirstOrDefault(o => o.Id == id)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        private OrganizationMember FindMembership(int id)
        {
            return _repository.GetMembers().FirstOrDefault(m => m.Id == id)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        private void EnsureNameAvailable(string name, int exceptId)
        {
            var normalized = name.Trim().ToLower();
            if (_repository.GetOrganizations().Any(o => o.Name.ToLower() == normalized && o.Id != exceptId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.OrganizationExists);
            }
        }
    }
}
