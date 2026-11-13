namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Departments (colleges) and the courses each one offers. A student's department comes from their
    /// course, so nothing extra is stored in the database. Edit this list to match the school: every
    /// course in <see cref="Courses.All"/> should appear under exactly one department.
    /// </summary>
    public static class Departments
    {
        /// <summary>Filter value for students whose course is not listed under any department.</summary>
        public const string OtherCode = "OTHER";

        public sealed class Department
        {
            public Department(string code, string name, params string[] courseNames)
            {
                Code = code;
                Name = name;
                CourseNames = courseNames;
            }

            /// <summary>Short code shown in filters, e.g. "CCS".</summary>
            public string Code { get; }

            /// <summary>Full name, e.g. "College of Computer Studies".</summary>
            public string Name { get; }

            /// <summary>Courses that belong to this department (same spelling as <see cref="Courses.All"/>).</summary>
            public IReadOnlyList<string> CourseNames { get; }
        }

        public static readonly Department[] All =
        {
            new Department("CAS", "College of Arts and Sciences",
                "AB Communication", "AB Political Science", "BS Psychology"),
            new Department("CBA", "College of Business and Accountancy",
                "BS Accountancy", "BS Business Administration"),
            new Department("CCS", "College of Computer Studies",
                "BS Computer Science", "BS Information Systems", "BS Information Technology"),
            new Department("COE", "College of Engineering",
                "BS Civil Engineering", "BS Computer Engineering", "BS Electrical Engineering", "BS Mechanical Engineering"),
            new Department("CHTM", "College of Hospitality and Tourism Management",
                "BS Hospitality Management", "BS Tourism Management"),
            new Department("CON", "College of Nursing",
                "BS Nursing"),
            new Department("CTE", "College of Teacher Education",
                "BEEd - Elementary Education", "BSEd - Secondary Education"),
        };

        /// <summary>Every course that belongs to some department.</summary>
        public static readonly IReadOnlyList<string> AllCourseNames = All.SelectMany(d => d.CourseNames).ToArray();

        /// <summary>The department with this code (any letter case), or null.</summary>
        public static Department? Find(string? code)
        {
            return All.FirstOrDefault(d => string.Equals(d.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The code of the department that offers this course, or null when no department lists it.</summary>
        public static string? GetCode(string? course)
        {
            if (string.IsNullOrEmpty(course)) return null;
            return All.FirstOrDefault(d => d.CourseNames.Contains(course))?.Code;
        }
    }
}
