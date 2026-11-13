using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPasswordResetRepository
    {
        IQueryable<PasswordResetRequest> GetRequests();
        void AddRequest(PasswordResetRequest entity);
        void UpdateRequest(PasswordResetRequest entity);
    }
}
