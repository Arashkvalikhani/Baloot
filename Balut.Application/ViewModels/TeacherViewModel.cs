using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class TeacherViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string NationalCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Expertise { get; set; }

        public string? Bio { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public int Status { get; set; } = 1;
    }
}