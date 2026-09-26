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
    }
}
