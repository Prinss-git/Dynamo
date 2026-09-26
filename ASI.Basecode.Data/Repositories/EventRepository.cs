using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class EventRepository : BaseRepository, IEventRepository
    {
        public EventRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<Event> GetEvents()
        {
            return this.GetDbSet<Event>();
        }

        public void AddEvent(Event entity)
        {
            this.GetDbSet<Event>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateEvent(Event entity)
        {
            this.GetDbSet<Event>().Update(entity);
            UnitOfWork.SaveChanges();
        }

        public void DeleteEvent(Event entity)
        {
            this.GetDbSet<Event>().Remove(entity);
            UnitOfWork.SaveChanges();
        }
    }
}
