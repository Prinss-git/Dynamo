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

        /// <summary>Self-registered accounts waiting for an administrator.</summary>
        List<UserModel> GetPendingUsers();
        int CountPendingUsers();
        /// <summary>Approves the given pending accounts; returns how many were approved.</summary>
        int ApproveUsers(IEnumerable<int> ids, string actor);
        int ApproveAllUsers(string actor);
        /// <summary>Deletes the given pending registration requests; returns how many were removed.</summary>
        int RejectUsers(IEnumerable<int> ids);

        /// <summary>Active user by primary key (development account switcher).</summary>
        User GetActiveUser(int id);
    }
}
