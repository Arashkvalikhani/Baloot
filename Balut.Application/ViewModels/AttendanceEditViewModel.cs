namespace Balut.Application.ViewModels
{
    public class AttendanceEditViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Status { get; set; } = 1; // 1: حاضر، 2: غایب، 3: تأخیر
        public int? LateMinutes { get; set; }
    }
}