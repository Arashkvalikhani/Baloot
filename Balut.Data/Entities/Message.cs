namespace Balut.Data.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public int? SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? Sender { get; set; }
        public ApplicationUser? Receiver { get; set; }
        public Student? Student { get; set; }
        public Session? Session { get; set; }
    }
}