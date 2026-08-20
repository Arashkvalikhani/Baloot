namespace Balut.Application.ViewModels
{
    public class ScoreReportViewModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int SessionNumber { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public decimal ScoreValue { get; set; }
        public string? Comments { get; set; }
        public DateTime Date { get; set; }
    }
}