using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IEventRegistrationRepository
    {
        IQueryable<EventRegistration> GetRegistrations();
        void AddRegistration(EventRegistration entity);
        void UpdateRegistration(EventRegistration entity);
    }
}
