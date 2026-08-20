using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class ClassViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کلاس الزامی است")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        public string? Room { get; set; }
        public string? Schedule { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Capacity { get; set; }

        public int Status { get; set; } = 1;

        public string? CourseTitle { get; set; }
        public string? TeacherName { get; set; }
        public int StudentCount { get; set; }
    }
}