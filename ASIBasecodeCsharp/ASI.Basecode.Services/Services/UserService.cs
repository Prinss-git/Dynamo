using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            if (user == null) return LoginResult.Failed;
            return user.IsActive ? LoginResult.Success : LoginResult.Inactive;
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (_repository.UserExists(model.UserId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
            EnsureStudentNumberAvailable(model.StudentNumber, 0);

            _mapper.Map(model, user);
            user.Role = Role.Student;
            user.IsActive = true;
            user.Password = PasswordManager.EncryptPassword(model.Password);
            user.CreatedTime = DateTime.Now;
            user.UpdatedTime = DateTime.Now;
            user.CreatedBy = model.UserId;
            user.UpdatedBy = model.UserId;

            _repository.AddUser(user);
        }

        public List<UserModel> GetUsers(string search, Role? role)
        {
            var query = _repository.GetUsers();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.UserId.ToLower().Contains(term)
                                      || u.Name.ToLower().Contains(term)
                                      || (u.StudentNumber != null && u.StudentNumber.ToLower().Contains(term)));
            }
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            return query.OrderBy(u => u.Role).ThenBy(u => u.Name)
                        .ToList()
                        .Select(u => _mapper.Map<UserModel>(u))
                        .ToList();
        }

        public UserModel GetUser(int id)
        {
            var user = FindUser(id);
            var model = _mapper.Map<UserModel>(user);
            model.Password = null;
            return model;
        }

        public void CreateUser(UserModel model, string actor)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                throw new InvalidDataException("Password is required.");
            }
            if (_repository.UserExists(model.UserId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
            EnsureStudentNumberAvailable(model.StudentNumber, 0);

            var user = _mapper.Map<User>(model);
            user.Id = 0;
            user.Password = PasswordManager.EncryptPassword(model.Password);
            user.CreatedTime = DateTime.Now;
            user.UpdatedTime = DateTime.Now;
            user.CreatedBy = actor;
            user.UpdatedBy = actor;

            _repository.AddUser(user);
        }

        public void UpdateUser(UserModel model, string actor)
        {
            var user = FindUser(model.Id);
            if (!string.Equals(user.UserId, model.UserId, StringComparison.Ordinal) && _repository.UserExists(model.UserId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
            EnsureStudentNumberAvailable(model.StudentNumber, model.Id);

            var currentPassword = user.Password;
            _mapper.Map(model, user);
            user.Password = string.IsNullOrWhiteSpace(model.Password)
                ? currentPassword
                : PasswordManager.EncryptPassword(model.Password);
            user.UpdatedTime = DateTime.Now;
            user.UpdatedBy = actor;

            _repository.UpdateUser(user);
        }

        public void ToggleActive(int id, string actor)
        {
            var user = FindUser(id);
            user.IsActive = !user.IsActive;
            user.UpdatedTime = DateTime.Now;
            user.UpdatedBy = actor;
            _repository.UpdateUser(user);
        }

        public List<OptionModel> GetUserOptions()
        {
            return _repository.GetUsers()
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .Select(u => new OptionModel
                {
                    Id = u.Id,
                    Text = u.Name + " (" + (u.StudentNumber ?? u.UserId) + ") - " + u.Role,
                })
                .ToList();
        }

        private User FindUser(int id)
        {
            return _repository.GetUsers().FirstOrDefault(u => u.Id == id)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        private void EnsureStudentNumberAvailable(string studentNumber, int exceptUserId)
        {
            if (string.IsNullOrWhiteSpace(studentNumber)) return;

            if (_repository.GetUsers().Any(u => u.StudentNumber == studentNumber && u.Id != exceptUserId))
            {
                throw new InvalidDataException(Resources.Messages.Errors.StudentNumberExists);
            }
        }
    }
}
