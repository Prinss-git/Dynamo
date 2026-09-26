using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class EventViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organization is required.")]
        [Display(Name = "Organization")]
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Venue is required.")]
        [StringLength(150)]
        public string Venue { get; set; }

        [Required]
        [Display(Name = "Starts")]
        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(7).AddHours(9);

        [Required]
        [Display(Name = "Ends")]
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(7).AddHours(12);

        [Required]
        [Display(Name = "Registration Deadline")]
        public DateTime RegistrationDeadline { get; set; } = DateTime.Today.AddDays(6).AddHours(17);

        /// <summary>0 means unlimited</summary>
        [Range(0, 100000, ErrorMessage = "Capacity must be 0 (unlimited) or more.")]
        public int Capacity { get; set; } = 50;

        [Range(0, 1440)]
        [Display(Name = "Late After (minutes)")]
        public int LateAfterMinutes { get; set; } = 15;

        public EventStatus Status { get; set; } = EventStatus.Draft;

        public int RegisteredCount { get; set; }
        public int WaitlistedCount { get; set; }
        public int AttendedCount { get; set; }

        /// <summary>Registration of the signed-in student, if any</summary>
        public int? MyRegistrationId { get; set; }
        public RegistrationStatus? MyRegistrationStatus { get; set; }

        /// <summary>Whether the signed-in user may edit this event / take attendance</summary>
        public bool CanManage { get; set; }

        public bool IsUnlimited => Capacity == 0;
        public int? SlotsLeft => IsUnlimited ? null : Math.Max(Capacity - RegisteredCount, 0);
        public bool IsRegistrationOpen => Status == EventStatus.Open && DateTime.Now <= RegistrationDeadline && DateTime.Now < StartTime;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(Resources.Messages.Errors.InvalidEventSchedule, new[] { nameof(EndTime) });
            }
            if (RegistrationDeadline > StartTime)
            {
                yield return new ValidationResult(Resources.Messages.Errors.InvalidDeadline, new[] { nameof(RegistrationDeadline) });
            }
        }
    }
}
