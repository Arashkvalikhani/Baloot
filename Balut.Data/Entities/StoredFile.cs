namespace Balut.Data.Entities
{
    public class StoredFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string? UploadedById { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}