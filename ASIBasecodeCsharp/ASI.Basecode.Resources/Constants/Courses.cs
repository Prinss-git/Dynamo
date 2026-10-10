namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Courses offered in the course dropdown. Edit this list to match the school's programs.
    /// </summary>
    public static class Courses
    {
        public static readonly string[] All =
        {
            "AB Communication",
            "AB Political Science",
            "BEEd - Elementary Education",
            "BSEd - Secondary Education",
            "BS Accountancy",
            "BS Business Administration",
            "BS Civil Engineering",
            "BS Computer Engineering",
            "BS Computer Science",
            "BS Electrical Engineering",
            "BS Hospitality Management",
            "BS Information Systems",
            "BS Information Technology",
            "BS Mechanical Engineering",
            "BS Nursing",
            "BS Psychology",
            "BS Tourism Management",
        };

        public static bool IsValid(string course)
        {
            return System.Array.IndexOf(All, course) >= 0;
        }
    }
}
