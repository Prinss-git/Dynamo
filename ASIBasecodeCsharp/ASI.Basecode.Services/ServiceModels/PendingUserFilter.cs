using System;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Filters for the pending account approvals list. Property names double as the query-string keys.
    /// </summary>
    public class PendingUserFilter
    {
        public const string SortOldest = "oldest";
        public const string SortNewest = "newest";

        /// <summary>Department code (see Departments.All), Departments.OtherCode, or empty for all departments.</summary>
        public string Department { get; set; }

        /// <summary>One course (see Courses.All); narrows the department filter further.</summary>
        public string Course { get; set; }

        public int? YearLevel { get; set; }

        /// <summary>Part of a name or a student number.</summary>
        public string Search { get; set; }

        /// <summary><see cref="SortNewest"/> lists the latest requests first; anything else lists the oldest first.</summary>
        public string Sort { get; set; }

        public bool NewestFirst => string.Equals(Sort, SortNewest, StringComparison.OrdinalIgnoreCase);

        /// <summary>True when the list is narrowed by department, year level or search (sorting does not count).</summary>
        public bool IsFiltered => !string.IsNullOrWhiteSpace(Department)
                               || !string.IsNullOrWhiteSpace(Course)
                               || YearLevel.HasValue
                               || !string.IsNullOrWhiteSpace(Search);
    }
}
