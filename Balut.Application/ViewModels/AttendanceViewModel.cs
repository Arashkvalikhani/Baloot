using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "وضعیت نامعتبر است")]
        public int Status { get; set; } // 1: Present, 2: Absent, 3: Late

        [Range(0, 120, ErrorMessage = "دقیقه تأخیر باید بین 0 تا 120 باشد")]
        public int? LateMinutes { get; set; }

        public string? Notes { get; set; }

        public string? StudentName { get; set; }
        public string? SessionTopic { get; set; }
    }
}