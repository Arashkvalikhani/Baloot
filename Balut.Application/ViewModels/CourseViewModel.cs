using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان دوره الزامی است")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "ظرفیت باید بین 1 تا 10000 باشد")]
        public int Capacity { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int NumberOfSessions { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
        public decimal Price { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Level { get; set; }
        public int Status { get; set; } = 1;
    }
}