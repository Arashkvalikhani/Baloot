namespace Balut.Data.Entities
{
    public class ExerciseSubmission
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int StudentId { get; set; }
        public string? TextContent { get; set; }
        public int Status { get; set; } = 1;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public decimal? Score { get; set; }
        public string? TeacherComment { get; set; }

        public Exercise? Exercise { get; set; }
        public Student? Student { get; set; }
    }
}