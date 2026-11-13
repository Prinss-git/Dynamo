namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Colleges (departments) and the courses under them, following the programs University of Cebu -
    /// Banilad lists on uc.edu.ph/baniladcampus, grouped by the colleges on uc.edu.ph/academics.
    /// Edit this list to match the school's programs; the course dropdowns, filters and event
    /// eligibility all read from it.
    /// </summary>
    public static class Courses
    {
        /// <summary>A college: short code (used in filter links), full name and its courses.</summary>
        public sealed record Department(string Code, string Name, string[] Courses);

        public static readonly Department[] Departments =
        {
            new("CCS", "College of Computer Studies", new[]
            {
                "BS Computer Science (AI-Focused Elective)",
                "BS Information Technology",
                "BS Information Technology - Software Engineering",
            }),
            new("COE", "College of Engineering", new[]
            {
                "BS Computer Engineering",
            }),
            new("CBA", "College of Business and Accountancy", new[]
            {
                "BS Accountancy",
                "BS Accountancy - Information Systems",
                "BS Business Administration - Financial Management",
                "BS Business Administration - Human Resources Management",
                "BS Business Administration - Marketing Management",
                "BS Business Administration - Operations Management",
                "BS Management Accounting",
            }),
            new("CED", "College of Education", new[]
            {
                "BEEd - Elementary Education",
                "BSEd - English",
            }),
            new("CAS", "College of Arts and Sciences", new[]
            {
                "BS Psychology",
            }),
            new("CCJE", "College of Criminal Justice Education", new[]
            {
                "BS Criminology",
            }),
            new("CON", "College of Nursing", new[]
            {
                "BS Nursing",
            }),
            new("CAHS", "College of Allied Health Sciences", new[]
            {
                "BS Medical Technology",
            }),
            new("CHTCA", "College of Hospitality, Tourism and Customs Administration", new[]
            {
                "BS Hospitality Management",
                "BS Tourism Management",
            }),
            new("LAW", "School of Law", new[]
            {
                "Juris Doctor",
            }),
        };

        public static readonly string[] All = Departments.SelectMany(d => d.Courses).ToArray();

        public static bool IsValid(string? course)
        {
            return course != null && System.Array.IndexOf(All, course) >= 0;
        }

        /// <summary>The department a course belongs to, or null for an unknown or empty course.</summary>
        public static string? DepartmentOf(string? course)
        {
            return Departments.FirstOrDefault(d => System.Array.IndexOf(d.Courses, course) >= 0)?.Name;
        }

        /// <summary>
        /// Short labels for a set of courses: a department's name when all of its courses are in the set,
        /// otherwise the individual course names. In course-list order.
        /// </summary>
        public static IEnumerable<string> Describe(IEnumerable<string>? courses)
        {
            var set = new HashSet<string>(courses ?? Enumerable.Empty<string>());
            foreach (var department in Departments)
            {
                var picked = department.Courses.Where(set.Contains).ToList();
                if (picked.Count == 0) continue;
                if (picked.Count == department.Courses.Length && department.Courses.Length > 1)
                {
                    yield return department.Name;
                }
                else
                {
                    foreach (var course in picked) yield return course;
                }
            }
        }

        /// <summary>The courses of a department (empty for an unknown department).</summary>
        public static string[] CoursesOf(string? department)
        {
            return Departments.FirstOrDefault(d => d.Name == department)?.Courses ?? System.Array.Empty<string>();
        }
    }
}
