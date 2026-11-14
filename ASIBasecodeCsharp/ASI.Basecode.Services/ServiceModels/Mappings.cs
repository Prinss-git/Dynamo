using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Copies between form models and database models. Written by hand (instead of AutoMapper) so every copied
    /// field is visible here; fields a form must never change (ids, status, passwords, approval) are not copied.
    /// </summary>
    public static class Mappings
    {
        /// <summary>Self-registration form onto a new user. Name, role, approval and password are set by the service.</summary>
        public static void ApplyTo(this UserViewModel model, User user)
        {
            user.UserId = model.UserId;
            user.FirstName = model.FirstName;
            user.MiddleName = model.MiddleName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.StudentNumber = model.StudentNumber;
            user.Course = model.Course;
            user.YearLevel = model.YearLevel;
        }

        /// <summary>Admin user form onto a user. Id, password, name and approval are handled by the service.</summary>
        public static void ApplyTo(this UserModel model, User user)
        {
            user.UserId = model.UserId;
            user.FirstName = model.FirstName;
            user.MiddleName = model.MiddleName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.StudentNumber = model.StudentNumber;
            user.Course = model.Course;
            user.YearLevel = model.YearLevel;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
        }

        /// <summary>User for the admin screens; the password is never copied out.</summary>
        public static UserModel ToModel(this User user)
        {
            return new UserModel
            {
                Id = user.Id,
                UserId = user.UserId,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Name = user.Name,
                Email = user.Email,
                StudentNumber = user.StudentNumber,
                Course = user.Course,
                YearLevel = user.YearLevel,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedTime = user.CreatedTime,
                IsApproved = user.IsApproved,
            };
        }

        public static void ApplyTo(this OrganizationViewModel model, Organization organization)
        {
            organization.Name = model.Name;
            organization.Acronym = model.Acronym;
            organization.Description = model.Description;
            organization.Adviser = model.Adviser;
            organization.IsActive = model.IsActive;
        }

        public static OrganizationViewModel ToModel(this Organization organization)
        {
            return new OrganizationViewModel
            {
                Id = organization.Id,
                Name = organization.Name,
                Acronym = organization.Acronym,
                Description = organization.Description,
                Adviser = organization.Adviser,
                IsActive = organization.IsActive,
            };
        }

        /// <summary>Event form onto an event. Status changes only through EventService.ChangeStatus; allowed courses are synced separately.</summary>
        public static void ApplyTo(this EventViewModel model, Event entity)
        {
            entity.OrganizationId = model.OrganizationId;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.Venue = model.Venue;
            entity.StartTime = model.StartTime;
            entity.EndTime = model.EndTime;
            entity.RegistrationDeadline = model.RegistrationDeadline;
            entity.Capacity = model.Capacity;
            entity.LateAfterMinutes = model.LateAfterMinutes;
        }
    }
}
