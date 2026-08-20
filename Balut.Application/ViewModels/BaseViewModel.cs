namespace Balut.Application.ViewModels
{
    public class AjaxResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class IdRequest
    {
        public int Id { get; set; }
    }

    public class CreateEnrollmentRequest
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
    }

    public class SaveAttendanceRequest
    {
        public int SessionId { get; set; }
        public List<AttendanceEditViewModel> Items { get; set; } = new();
    }
}