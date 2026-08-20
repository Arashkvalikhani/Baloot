namespace Balut.Application.ViewModels
{
    public class FileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
    }

    public class ExerciseViewModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public int Status { get; set; } = 1;
        public int? AttachmentId { get; set; }
        public string? AttachmentName { get; set; }
    }

    public class CreateExerciseRequest
    {
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class SubmissionViewModel
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? TextContent { get; set; }
        public int Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? Score { get; set; }
        public string? TeacherComment { get; set; }
        public int? AttachmentId { get; set; }
        public string? AttachmentName { get; set; }
    }

    public class GradeSubmissionRequest
    {
        public int SubmissionId { get; set; }
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
    }

    public class StudentExerciseViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public int SessionNumber { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? ExerciseAttachmentId { get; set; }
        public string? ExerciseAttachmentName { get; set; }
        public int? SubmissionId { get; set; }
        public string? SubmissionText { get; set; }
        public int? SubmissionStatus { get; set; }
        public decimal? SubmissionScore { get; set; }
        public string? TeacherComment { get; set; }
        public int? SubmissionAttachmentId { get; set; }
        public string? SubmissionAttachmentName { get; set; }
    }
}