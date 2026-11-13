using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// The account approvals screen: the filtered pending accounts plus the per-department counts.
    /// </summary>
    public class PendingApprovalsModel
    {
        public PendingUserFilter Filter { get; set; } = new();

        /// <summary>Pending accounts that match <see cref="Filter"/>.</summary>
        public List<UserModel> Users { get; set; } = new();

        /// <summary>All pending accounts, before filtering.</summary>
        public int TotalPending { get; set; }

        /// <summary>Pending accounts per department code (Departments.OtherCode for unlisted courses).</summary>
        public Dictionary<string, int> DepartmentCounts { get; set; } = new();

        /// <summary>Highest account id in <see cref="Users"/>; bulk actions stop there so later sign-ups are not included.</summary>
        public int ThroughId => Users.Count == 0 ? 0 : Users.Max(u => u.Id);

        public int CountFor(string departmentCode)
        {
            return departmentCode != null && DepartmentCounts.TryGetValue(departmentCode, out var count) ? count : 0;
        }
    }
}
