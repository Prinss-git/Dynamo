using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class AttendanceRepository : BaseRepository, IAttendanceRepository
    {
        public AttendanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<Attendance> GetAttendances()
        {
            return this.GetDbSet<Attendance>();
        }

        public void AddAttendance(Attendance entity)
        {
            this.GetDbSet<Attendance>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateAttendance(Attendance entity)
        {
            this.GetDbSet<Attendance>().Update(entity);
            UnitOfWork.SaveChanges();
        }

        public void AddAttendances(IEnumerable<Attendance> entities)
        {
            this.GetDbSet<Attendance>().AddRange(entities);
            UnitOfWork.SaveChanges();
        }
    }
}
