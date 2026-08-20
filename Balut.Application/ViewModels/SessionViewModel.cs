using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class SessionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int SessionNumber { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(200)]
        public string? Topic { get; set; }

        public string? Description { get; set; }

        public int Status { get; set; } = 1; // 1: Scheduled, 2: Completed, 3: Cancelled, 4: Postponed

        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
    }
}