using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class RegistrationService : IRegistrationService
    {
        // No 0/O or 1/I so codes are easy to read out and type in.
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 10;

        private readonly IEventRegistrationRepository _repository;
        private readonly IEventRepository _eventRepository;
        private readonly IAccessService _accessService;

        public RegistrationService(IEventRegistrationRepository repository,
                                   IEventRepository eventRepository,
                                   IAccessService accessService)
        {
            _repository = repository;
            _eventRepository = eventRepository;
            _accessService = accessService;
        }

        public RegistrationStatus Register(int eventId, CurrentUserModel currentUser)
        {
            var ev = _eventRepository.GetEvents().FirstOrDefault(e => e.Id == eventId && e.Status != EventStatus.Draft)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            var now = DateTime.Now;
            if (ev.Status != EventStatus.Open || now > ev.RegistrationDeadline || now >= ev.StartTime)
            {
                throw new InvalidDataException(Resources.Messages.Errors.RegistrationClosed);
            }

            var registeredCount = _repository.GetRegistrations()
                .Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered);
            var status = ev.Capacity > 0 && registeredCount >= ev.Capacity
                ? RegistrationStatus.Waitlisted
                : RegistrationStatus.Registered;

            var existing = _repository.GetRegistrations()
                .FirstOrDefault(r => r.EventId == eventId && r.StudentId == currentUser.AccountId);
            if (existing != null)
            {
                if (existing.Status != RegistrationStatus.Cancelled)
                {
                    throw new InvalidDataException(Resources.Messages.Errors.AlreadyRegistered);
                }

                // Re-registering after a cancellation reuses the row (one row per student per event).
                existing.Status = status;
                existing.RegisteredTime = now;
                existing.CancelledTime = null;
                _repository.UpdateRegistration(existing);
                return status;
            }

            _repository.AddRegistration(new EventRegistration
            {
                EventId = eventId,
                StudentId = currentUser.AccountId,
                RegistrationCode = GenerateUniqueCode(),
                Status = status,
                RegisteredTime = now,
            });
            return status;
        }

        public void Cancel(int registrationId, CurrentUserModel currentUser)
        {
            var registration = _repository.GetRegistrations()
                .Where(r => r.Id == registrationId)
                .Select(RegistrationProjection.ToViewModel)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            EnsureOwnerOrManager(registration, currentUser);
            if (!registration.CanCancel)
            {
                throw new InvalidDataException(Resources.Messages.Errors.CannotCancelRegistration);
            }

            var entity = _repository.GetRegistrations().First(r => r.Id == registrationId);
            var wasRegistered = entity.Status == RegistrationStatus.Registered;
            entity.Status = RegistrationStatus.Cancelled;
            entity.CancelledTime = DateTime.Now;
            _repository.UpdateRegistration(entity);

            if (wasRegistered)
            {
                PromoteWaitlisted(entity.EventId);
            }
        }

        public List<RegistrationViewModel> GetMyRegistrations(int accountId)
        {
            return _repository.GetRegistrations()
                .Where(r => r.StudentId == accountId)
                .OrderByDescending(r => r.Event.StartTime)
                .Select(RegistrationProjection.ToViewModel)
                .ToList();
        }

        public RegistrationViewModel GetTicket(int registrationId, CurrentUserModel currentUser)
        {
            var registration = _repository.GetRegistrations()
                .Where(r => r.Id == registrationId)
                .Select(RegistrationProjection.ToViewModel)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);

            EnsureOwnerOrManager(registration, currentUser);
            if (registration.Status == RegistrationStatus.Registered)
            {
                registration.QrCodeSvg = QrCodeManager.GenerateSvg(registration.RegistrationCode);
            }
            return registration;
        }

        public void PromoteWaitlisted(int eventId)
        {
            var ev = _eventRepository.GetEvents().FirstOrDefault(e => e.Id == eventId);
            if (ev == null || (ev.Status != EventStatus.Open && ev.Status != EventStatus.Closed))
            {
                return;
            }

            var waitlist = _repository.GetRegistrations()
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.RegisteredTime)
                .ToList();
            if (waitlist.Count == 0) return;

            var freeSlots = waitlist.Count;
            if (ev.Capacity > 0)
            {
                var registeredCount = _repository.GetRegistrations()
                    .Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered);
                freeSlots = Math.Max(ev.Capacity - registeredCount, 0);
            }

            foreach (var registration in waitlist.Take(freeSlots))
            {
                registration.Status = RegistrationStatus.Registered;
                _repository.UpdateRegistration(registration);
            }
        }

        private void EnsureOwnerOrManager(RegistrationViewModel registration, CurrentUserModel currentUser)
        {
            if (registration.StudentId == currentUser.AccountId) return;

            var organizationId = _eventRepository.GetEvents()
                .Where(e => e.Id == registration.EventId)
                .Select(e => e.OrganizationId)
                .First();
            _accessService.EnsureCanManageOrganization(organizationId, currentUser);
        }

        private string GenerateUniqueCode()
        {
            string code;
            do
            {
                var chars = new char[CodeLength];
                for (var i = 0; i < CodeLength; i++)
                {
                    chars[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
                }
                code = new string(chars);
            }
            while (_repository.GetRegistrations().Any(r => r.RegistrationCode == code));

            return code;
        }
    }
}
