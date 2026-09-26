using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOrganizationService
    {
        List<OrganizationViewModel> GetOrganizations(string search);
        OrganizationViewModel GetOrganization(int id);
        void CreateOrganization(OrganizationViewModel model, string actor);
        void UpdateOrganization(OrganizationViewModel model, string actor);
        void DeleteOrganization(int id);

        OrganizationMembersModel GetMembers(int organizationId);
        void AddMember(int organizationId, int memberId, string position);
        void UpdateMemberPosition(int membershipId, string position);
        int RemoveMember(int membershipId);

        /// <summary>Active organizations the user may create events for</summary>
        List<OptionModel> GetManagedOrganizationOptions(CurrentUserModel currentUser);
    }
}
