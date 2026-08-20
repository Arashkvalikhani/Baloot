namespace Balut.Application.ViewModels
{
    public class AttendanceReportViewModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int SessionNumber { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int Status { get; set; }
        public int? LateMinutes { get; set; }
        public DateTime Date { get; set; }
    }
}