using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPasswordResetRepository _resetRepository;

        public UserService(IUserRepository repository, IPasswordResetRepository resetRepository)
        {
            _repository = repository;
            _resetRepository = resetRepository;
        }

        /// <summary>
        /// Checks a username and password. Wrong username and wrong password give the same result (Failed),
        /// so the login page never reveals which usernames exist.
        /// </summary>
        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            // STEP 1: find the account and compare the password with the stored hash (see PasswordManager)
            user = _repository.GetUsers().FirstOrDefault(x => x.UserId == userId);
            if (user == null || !PasswordManager.VerifyPassword(user.Password, password, out var needsUpgrade))
            {
                user = null;
                return LoginResult.Failed;
            }

            // old template/demo passwords (reversible encryption) become a one-way hash at the first sign-in
            if (needsUpgrade)
            {
                user.Password = PasswordManager.HashPassword(password);
                _repository.UpdateUser(user);
            }

            // STEP 2: the password is right, but the account must be approved by an admin and still active
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

            model.ApplyTo(user);
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
            user.Role = Role.Student;
            user.IsActive = true;
            user.IsApproved = false;
            user.Password = PasswordManager.HashPassword(model.Password);
            user.CreatedTime = AppClock.Now;
            user.UpdatedTime = AppClock.Now;
            user.CreatedBy = model.UserId;
            user.UpdatedBy = model.UserId;

            _repository.AddUser(user);
        }

        public List<UserModel> GetUsers(string search, Role? role, string department = null, string course = null, bool? active = null, string sort = null)
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
            if (active.HasValue)
            {
                query = query.Where(u => u.IsActive == active.Value);
            }
            query = FilterByCourse(query, department, course);

            var ordered = sort switch
            {
                Const.SortNewest => query.OrderByDescending(u => u.CreatedTime).ThenByDescending(u => u.Id),
                Const.SortOldest => query.OrderBy(u => u.CreatedTime).ThenBy(u => u.Id),
                _ => query.OrderBy(u => u.Role).ThenBy(u => u.Name),
            };
            return ordered.ToList()
                        .Select(u => u.ToModel())
                        .ToList();
        }

        public UserModel GetUser(int id)
        {
            var user = FindUser(id);
            var model = user.ToModel();
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

            var user = new User();
            model.ApplyTo(user);
            user.Id = 0;
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
            user.IsApproved = true;
            user.ApprovedBy = actor;
            user.ApprovedTime = AppClock.Now;
            user.Password = PasswordManager.HashPassword(model.Password);
            user.CreatedTime = AppClock.Now;
            user.UpdatedTime = AppClock.Now;
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
            model.ApplyTo(user);
            SetName(user, model.FirstName, model.MiddleName, model.LastName);
            user.Password = string.IsNullOrWhiteSpace(model.Password)
                ? currentPassword
                : PasswordManager.HashPassword(model.Password);
            user.UpdatedTime = AppClock.Now;
            user.UpdatedBy = actor;

            _repository.UpdateUser(user);
        }

        public void ToggleActive(int id, string actor)
        {
            var user = FindUser(id);
            user.IsActive = !user.IsActive;
            user.UpdatedTime = AppClock.Now;
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
                        .Select(u => u.ToModel())
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

            if (Courses.IsValid(filter.Course))
            {
                var course = filter.Course;
                query = query.Where(u => u.Course == course);
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

            var now = AppClock.Now;
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

        public void RequestPasswordReset(string userId, string studentNumberOrEmail)
        {
            var username = (userId ?? string.Empty).Trim();
            var proof = (studentNumberOrEmail ?? string.Empty).Trim();
            if (username.Length == 0 || proof.Length == 0) return;
            var proofLower = proof.ToLower();
            // students prove it with their student number; officers and admins (usually no student number) with their email
            var user = _repository.GetUsers().FirstOrDefault(u => u.UserId == username && u.IsActive && u.IsApproved
                                                                && (u.StudentNumber == proof
                                                                    || (u.Email != null && u.Email.ToLower() == proofLower)));
            if (user == null) return;
            if (_resetRepository.GetRequests().Any(r => r.UserId == user.Id && r.Status == PasswordResetStatus.Pending)) return;

            _resetRepository.AddRequest(new PasswordResetRequest
            {
                UserId = user.Id,
                RequestedTime = AppClock.Now,
                Status = PasswordResetStatus.Pending,
            });
        }

        public List<PasswordResetRequestModel> GetPendingResets()
        {
            return PendingResetQuery().OrderBy(r => r.RequestedTime).ToList();
        }

        public int CountPendingResets()
        {
            return _resetRepository.GetRequests().Count(r => r.Status == PasswordResetStatus.Pending);
        }

        public PasswordResetRequestModel GetPendingReset(int requestId)
        {
            return PendingResetQuery().FirstOrDefault(r => r.Id == requestId)
                ?? throw new InvalidDataException(Resources.Messages.Errors.PasswordResetNotPending);
        }

        public string CompletePasswordReset(int requestId, string temporaryPassword, string actor)
        {
            var request = FindPendingReset(requestId);
            var user = FindUser(request.UserId);

            var now = AppClock.Now;
            user.Password = PasswordManager.HashPassword(temporaryPassword);
            user.MustChangePassword = true;
            user.UpdatedBy = actor;
            user.UpdatedTime = now;
            _repository.UpdateUser(user);

            request.Status = PasswordResetStatus.Completed;
            request.HandledBy = actor;
            request.HandledTime = now;
            _resetRepository.UpdateRequest(request);
            return user.UserId;
        }

        public void DismissPasswordReset(int requestId, string actor)
        {
            var request = FindPendingReset(requestId);
            request.Status = PasswordResetStatus.Dismissed;
            request.HandledBy = actor;
            request.HandledTime = AppClock.Now;
            _resetRepository.UpdateRequest(request);
        }

        public User ChangeOwnPassword(int accountId, string currentPassword, string newPassword)
        {
            var user = FindUser(accountId);
            if (!PasswordManager.VerifyPassword(user.Password, currentPassword))
            {
                throw new InvalidDataException(Resources.Messages.Errors.CurrentPasswordIncorrect);
            }
            if (PasswordManager.VerifyPassword(user.Password, newPassword))
            {
                // also stops a student from keeping the temporary password an admin gave them
                throw new InvalidDataException(Resources.Messages.Errors.PasswordSameAsCurrent);
            }

            user.Password = PasswordManager.HashPassword(newPassword);
            user.MustChangePassword = false;
            user.UpdatedBy = user.UserId;
            user.UpdatedTime = AppClock.Now;
            _repository.UpdateUser(user);
            return user;
        }

        public User GetSessionUser(int accountId)
        {
            return _repository.GetUsers().AsNoTracking().FirstOrDefault(u => u.Id == accountId);
        }

        private IQueryable<PasswordResetRequestModel> PendingResetQuery()
        {
            return _resetRepository.GetRequests()
                .Where(r => r.Status == PasswordResetStatus.Pending)
                .Select(r => new PasswordResetRequestModel
                {
                    Id = r.Id,
                    AccountId = r.UserId,
                    UserId = r.User.UserId,
                    Name = r.User.Name,
                    StudentNumber = r.User.StudentNumber,
                    Course = r.User.Course,
                    YearLevel = r.User.YearLevel,
                    Email = r.User.Email,
                    RequestedTime = r.RequestedTime,
                });
        }

        private PasswordResetRequest FindPendingReset(int requestId)
        {
            return _resetRepository.GetRequests().FirstOrDefault(r => r.Id == requestId && r.Status == PasswordResetStatus.Pending)
                ?? throw new InvalidDataException(Resources.Messages.Errors.PasswordResetNotPending);
        }

        /// <summary>
        /// Keeps users of one course, or of every course in a department code (see Departments.All); a course wins
        /// over a department.
        /// </summary>
        private static IQueryable<User> FilterByCourse(IQueryable<User> query, string department, string course)
        {
            if (Courses.IsValid(course))
            {
                return query.Where(u => u.Course == course);
            }
            if (string.IsNullOrWhiteSpace(department))
            {
                return query;
            }
            if (string.Equals(department.Trim(), Departments.OtherCode, StringComparison.OrdinalIgnoreCase))
            {
                // courses no department lists (or none at all)
                var listed = Departments.AllCourseNames.ToList();
                return query.Where(u => u.Course == null || !listed.Contains(u.Course));
            }
            // an unknown department code matches nobody instead of silently showing everyone
            var courses = Departments.Find(department)?.CourseNames.ToList() ?? new List<string>();
            return query.Where(u => courses.Contains(u.Course));
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
