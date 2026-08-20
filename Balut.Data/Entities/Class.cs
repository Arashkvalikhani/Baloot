namespace Balut.Data.Entities
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public string? Room { get; set; }
        public string? Schedule { get; set; }
        public int Capacity { get; set; }
        public int Status { get; set; } = 1;

        public Course? Course { get; set; }
        public Teacher? Teacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}