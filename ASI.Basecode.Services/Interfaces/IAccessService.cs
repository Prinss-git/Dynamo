using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Decides which organizations (and therefore events) a user may manage.
    /// Admins manage everything; Officers manage organizations they are members of.
    /// </summary>
    public interface IAccessService
    {
        bool CanManageOrganization(int organizationId, CurrentUserModel currentUser);
        void EnsureCanManageOrganization(int organizationId, CurrentUserModel currentUser);
        List<int> GetManagedOrganizationIds(CurrentUserModel currentUser);
    }
}
