using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
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
            if (!user.IsApproved) return LoginResult.Pending;
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
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
            user.Role = Role.Student;
            user.IsActive = true;
            user.IsApproved = false;
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
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
            user.IsApproved = true;
            user.ApprovedBy = actor;
            user.ApprovedTime = DateTime.Now;
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
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
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
                .Where(u => u.IsActive && u.IsApproved)
                .OrderBy(u => u.Name)
                .Select(u => new OptionModel
                {
                    Id = u.Id,
                    Text = u.Name + " (" + (u.StudentNumber ?? u.UserId) + ") - " + u.Role,
                })
                .ToList();
        }

        public User GetActiveUser(int id)
        {
            return _repository.GetUsers().FirstOrDefault(u => u.Id == id && u.IsActive && u.IsApproved)
                ?? throw new KeyNotFoundException(Resources.Messages.Errors.NotFound);
        }

        public List<UserModel> GetPendingUsers(PendingUserFilter filter)
        {
            var query = FilterPending(filter);
            query = filter != null && filter.NewestFirst
                ? query.OrderByDescending(u => u.CreatedTime).ThenByDescending(u => u.Id)
                : query.OrderBy(u => u.CreatedTime).ThenBy(u => u.Id);

            return query.ToList()
                        .Select(u => _mapper.Map<UserModel>(u))
                        .ToList();
        }

        public int CountPendingUsers()
        {
            return _repository.GetUsers().Count(u => !u.IsApproved);
        }

        public Dictionary<string, int> CountPendingUsersByDepartment()
        {
            var perCourse = _repository.GetUsers()
                .Where(u => !u.IsApproved)
                .GroupBy(u => u.Course)
                .Select(g => new { Course = g.Key, Count = g.Count() })
                .ToList();

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in perCourse)
            {
                var code = Departments.GetCode(row.Course) ?? Departments.OtherCode;
                counts[code] = (counts.TryGetValue(code, out var count) ? count : 0) + row.Count;
            }
            return counts;
        }

        public int ApproveUsers(IEnumerable<int> ids, string actor)
        {
            var idList = (ids ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (idList.Count == 0)
            {
                throw new InvalidDataException(Resources.Messages.Errors.NoUsersSelected);
            }
            return Approve(_repository.GetUsers().Where(u => !u.IsApproved && idList.Contains(u.Id)).ToList(), actor);
        }

        public int ApproveAllUsers(PendingUserFilter filter, int throughId, string actor)
        {
            return Approve(FilterPending(filter).Where(u => u.Id <= throughId).ToList(), actor);
        }

        public int RejectUsers(IEnumerable<int> ids)
        {
            var idList = (ids ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (idList.Count == 0)
            {
                throw new InvalidDataException(Resources.Messages.Errors.NoUsersSelected);
            }

            // Only pending requests can be rejected; approved accounts are never deleted here.
            var pending = _repository.GetUsers().Where(u => !u.IsApproved && idList.Contains(u.Id)).ToList();
            if (pending.Count > 0)
            {
                _repository.DeleteUsers(pending);
            }
            return pending.Count;
        }

        public int RejectAllUsers(PendingUserFilter filter, int throughId)
        {
            var pending = FilterPending(filter).Where(u => u.Id <= throughId).ToList();
            if (pending.Count > 0)
            {
                _repository.DeleteUsers(pending);
            }
            return pending.Count;
        }

        /// <summary>
        /// Pending (unapproved) accounts narrowed by department, year level and name / student number search.
        /// </summary>
        private IQueryable<User> FilterPending(PendingUserFilter filter)
        {
            var query = _repository.GetUsers().Where(u => !u.IsApproved);
            if (filter == null) return query;

            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                if (string.Equals(filter.Department.Trim(), Departments.OtherCode, StringComparison.OrdinalIgnoreCase))
                {
                    var listedCourses = Departments.AllCourseNames.ToList();
                    query = query.Where(u => u.Course == null || !listedCourses.Contains(u.Course));
                }
                else
                {
                    // an unknown code matches nobody rather than silently showing everyone
                    var courses = Departments.Find(filter.Department)?.CourseNames.ToList() ?? new List<string>();
                    query = query.Where(u => courses.Contains(u.Course));
                }
            }

            if (filter.YearLevel.HasValue)
            {
                var yearLevel = filter.YearLevel.Value;
                query = query.Where(u => u.YearLevel == yearLevel);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(term)
                                      || (u.StudentNumber != null && u.StudentNumber.ToLower().Contains(term)));
            }

            return query;
        }

        private int Approve(List<User> users, string actor)
        {
            if (users.Count == 0) return 0;

            var now = DateTime.Now;
            foreach (var user in users)
            {
                user.IsApproved = true;
                user.ApprovedBy = actor;
                user.ApprovedTime = now;
                user.UpdatedBy = actor;
                user.UpdatedTime = now;
            }
            _repository.UpdateUsers(users);
            return users.Count;
        }

        /// <summary>
        /// Stores the name parts and the composed "First Middle Last" display name.
        /// </summary>
        private static void SetName(User user, string firstName, string middleName, string lastName)
        {
            user.FirstName = firstName?.Trim();
            user.MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
            user.LastName = lastName?.Trim();
            user.Name = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName }
                .Where(part => !string.IsNullOrEmpty(part)));
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
