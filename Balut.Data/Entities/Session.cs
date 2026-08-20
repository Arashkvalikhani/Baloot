namespace Balut.Data.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int SessionNumber { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Topic { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; } = 1; // 1: Scheduled, 2: Completed, 3: Cancelled, 4: Postponed

        public Class? Class { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}