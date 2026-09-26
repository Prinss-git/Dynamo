using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IOrganizationRepository
    {
        IQueryable<Organization> GetOrganizations();
        void AddOrganization(Organization entity);
        void UpdateOrganization(Organization entity);
        void DeleteOrganization(Organization entity);
        IQueryable<OrganizationMember> GetMembers();
        void AddMember(OrganizationMember entity);
        void UpdateMember(OrganizationMember entity);
        void DeleteMember(OrganizationMember entity);
    }
}
