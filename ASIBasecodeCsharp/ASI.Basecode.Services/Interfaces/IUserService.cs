using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        void AddUser(UserViewModel model);

        List<UserModel> GetUsers(string search, Role? role);
        UserModel GetUser(int id);
        void CreateUser(UserModel model, string actor);
        void UpdateUser(UserModel model, string actor);
        void ToggleActive(int id, string actor);
        List<OptionModel> GetUserOptions();

        /// <summary>Self-registered accounts waiting for an administrator, narrowed and sorted by the filter.</summary>
        List<UserModel> GetPendingUsers(PendingUserFilter filter);
        int CountPendingUsers();
        /// <summary>Pending accounts per department code (Departments.OtherCode for courses no department lists).</summary>
        Dictionary<string, int> CountPendingUsersByDepartment();
        /// <summary>Approves the given pending accounts; returns how many were approved.</summary>
        int ApproveUsers(IEnumerable<int> ids, string actor);
        /// <summary>
        /// Approves every pending account that matches the filter and has an id up to <paramref name="throughId"/>
        /// (the accounts that were on screen); returns how many were approved.
        /// </summary>
        int ApproveAllUsers(PendingUserFilter filter, int throughId, string actor);
        /// <summary>Deletes the given pending registration requests; returns how many were removed.</summary>
        int RejectUsers(IEnumerable<int> ids);
        /// <summary>
        /// Deletes every pending request that matches the filter and has an id up to <paramref name="throughId"/>;
        /// returns how many were removed.
        /// </summary>
        int RejectAllUsers(PendingUserFilter filter, int throughId);

        /// <summary>Active user by primary key (development account switcher).</summary>
        User GetActiveUser(int id);
    }
}
