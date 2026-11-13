using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PasswordResetRepository : BaseRepository, IPasswordResetRepository
    {
        public PasswordResetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<PasswordResetRequest> GetRequests()
        {
            return this.GetDbSet<PasswordResetRequest>();
        }

        public void AddRequest(PasswordResetRequest entity)
        {
            this.GetDbSet<PasswordResetRequest>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateRequest(PasswordResetRequest entity)
        {
            this.GetDbSet<PasswordResetRequest>().Update(entity);
            UnitOfWork.SaveChanges();
        }
    }
}
