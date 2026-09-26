using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAttendanceService
    {
        AttendanceSheetModel GetAttendanceSheet(int eventId, CurrentUserModel currentUser);

        /// <summary>Checks in by registration code (QR) or student number.</summary>
        CheckInResultModel CheckIn(int eventId, string codeOrStudentNumber, CurrentUserModel currentUser);
        void CheckOut(int registrationId, CurrentUserModel currentUser);
        void SetStatus(int registrationId, AttendanceStatus status, string remarks, CurrentUserModel currentUser);

        /// <summary>Records every registered attendee without a check-in as Absent.</summary>
        void MarkAbsentees(int eventId, string actor);
    }
}
