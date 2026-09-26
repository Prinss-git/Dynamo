using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class EventRegistrationRepository : BaseRepository, IEventRegistrationRepository
    {
        public EventRegistrationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<EventRegistration> GetRegistrations()
        {
            return this.GetDbSet<EventRegistration>();
        }

        public void AddRegistration(EventRegistration entity)
        {
            this.GetDbSet<EventRegistration>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateRegistration(EventRegistration entity)
        {
            this.GetDbSet<EventRegistration>().Update(entity);
            UnitOfWork.SaveChanges();
        }
    }
}
