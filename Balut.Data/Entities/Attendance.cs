namespace Balut.Data.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public int Status { get; set; } // 1: Present, 2: Absent, 3: Late
        public int? LateMinutes { get; set; }
        public string? Notes { get; set; }

        public Session? Session { get; set; }
        public Student? Student { get; set; }
    }
}