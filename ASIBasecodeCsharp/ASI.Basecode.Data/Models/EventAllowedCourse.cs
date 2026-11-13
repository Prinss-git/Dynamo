namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// A course whose students may register for an event. An event with no rows is open to all students.
    /// </summary>
    public partial class EventAllowedCourse
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Course { get; set; }

        public virtual Event Event { get; set; }
    }
}
