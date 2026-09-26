using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRegistrationService
    {
        /// <summary>Registers the user; returns Waitlisted when the event is full.</summary>
        RegistrationStatus Register(int eventId, CurrentUserModel currentUser);
        void Cancel(int registrationId, CurrentUserModel currentUser);
        List<RegistrationViewModel> GetMyRegistrations(int accountId);
        RegistrationViewModel GetTicket(int registrationId, CurrentUserModel currentUser);

        /// <summary>Moves the oldest waitlisted registrations into free capacity.</summary>
        void PromoteWaitlisted(int eventId);
    }
}
