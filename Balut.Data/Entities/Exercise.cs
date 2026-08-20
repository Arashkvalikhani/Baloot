namespace Balut.Data.Entities
{
    public class Exercise
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public int Status { get; set; } = 1;

        public Session? Session { get; set; }
        public ICollection<ExerciseSubmission> Submissions { get; set; } = new List<ExerciseSubmission>();
    }
}