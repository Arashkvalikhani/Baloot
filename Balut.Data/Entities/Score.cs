namespace Balut.Data.Entities
{
    public class Score
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; }

        public Session? Session { get; set; }
        public Student? Student { get; set; }
    }
}