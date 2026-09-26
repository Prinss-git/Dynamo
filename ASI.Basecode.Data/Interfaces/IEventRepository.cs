using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IEventRepository
    {
        IQueryable<Event> GetEvents();
        void AddEvent(Event entity);
        void UpdateEvent(Event entity);
        void DeleteEvent(Event entity);
    }
}
