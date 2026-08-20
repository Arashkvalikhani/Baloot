namespace Balut.Data.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int Duration { get; set; }
        public int NumberOfSessions { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Level { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}