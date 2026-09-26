using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class AccessService : IAccessService
    {
        private readonly IOrganizationRepository _organizationRepository;

        public AccessService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public bool CanManageOrganization(int organizationId, CurrentUserModel currentUser)
        {
            if (currentUser.Role == Role.Admin) return true;
            if (currentUser.Role != Role.Officer) return false;

            return _organizationRepository.GetMembers()
                .Any(m => m.OrganizationId == organizationId && m.MemberId == currentUser.AccountId);
        }

        public void EnsureCanManageOrganization(int organizationId, CurrentUserModel currentUser)
        {
            if (!CanManageOrganization(organizationId, currentUser))
            {
                throw new UnauthorizedAccessException(Resources.Messages.Errors.NotAuthorized);
            }
        }

        public List<int> GetManagedOrganizationIds(CurrentUserModel currentUser)
        {
            if (currentUser.Role == Role.Admin)
            {
                return _organizationRepository.GetOrganizations().Select(o => o.Id).ToList();
            }
            if (currentUser.Role != Role.Officer)
            {
                return new List<int>();
            }

            return _organizationRepository.GetMembers()
                .Where(m => m.MemberId == currentUser.AccountId)
                .Select(m => m.OrganizationId)
                .ToList();
        }
    }
}
