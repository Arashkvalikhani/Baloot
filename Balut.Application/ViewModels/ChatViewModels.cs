namespace Balut.Application.ViewModels
{
    public class PartnerViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; }
    }

    public class SendMessageRequest
    {
        public string ReceiverId { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}