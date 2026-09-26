using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEventService
    {
        /// <summary>Events visible to students (everything except drafts)</summary>
        List<EventViewModel> GetPublishedEvents(CurrentUserModel currentUser, string search, bool upcomingOnly);

        /// <summary>Events of the organizations the user manages</summary>
        List<EventViewModel> GetManagedEvents(CurrentUserModel currentUser, string search, EventStatus? status);

        EventViewModel GetEvent(int id, CurrentUserModel currentUser);
        int CreateEvent(EventViewModel model, CurrentUserModel currentUser);
        void UpdateEvent(EventViewModel model, CurrentUserModel currentUser);
        void DeleteEvent(int id, CurrentUserModel currentUser);
        void ChangeStatus(int id, EventStatus status, CurrentUserModel currentUser);
        List<EventStatus> GetAllowedStatusChanges(EventStatus current);
    }
}
