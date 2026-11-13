namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Departments (colleges) and the courses each one offers, read from <see cref="Courses.Departments"/> so the
    /// school's program list lives in one place. A student's department comes from their course, so nothing
    /// extra is stored in the database.
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
            Courses.Departments.Select(d => new Department(d.Code, d.Name, d.Courses)).ToArray();

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
