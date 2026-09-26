using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAttendanceRepository
    {
        IQueryable<Attendance> GetAttendances();
        void AddAttendance(Attendance entity);
        void UpdateAttendance(Attendance entity);
        void AddAttendances(IEnumerable<Attendance> entities);
    }
}
