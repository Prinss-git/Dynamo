using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class OrganizationRepository : BaseRepository, IOrganizationRepository
    {
        public OrganizationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<Organization> GetOrganizations()
        {
            return this.GetDbSet<Organization>();
        }

        public void AddOrganization(Organization entity)
        {
            this.GetDbSet<Organization>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateOrganization(Organization entity)
        {
            this.GetDbSet<Organization>().Update(entity);
            UnitOfWork.SaveChanges();
        }

        public void DeleteOrganization(Organization entity)
        {
            this.GetDbSet<Organization>().Remove(entity);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<OrganizationMember> GetMembers()
        {
            return this.GetDbSet<OrganizationMember>();
        }

        public void AddMember(OrganizationMember entity)
        {
            this.GetDbSet<OrganizationMember>().Add(entity);
            UnitOfWork.SaveChanges();
        }

        public void UpdateMember(OrganizationMember entity)
        {
            this.GetDbSet<OrganizationMember>().Update(entity);
            UnitOfWork.SaveChanges();
        }

        public void DeleteMember(OrganizationMember entity)
        {
            this.GetDbSet<OrganizationMember>().Remove(entity);
            UnitOfWork.SaveChanges();
        }
    }
}
