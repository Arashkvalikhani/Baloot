using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(100, ErrorMessage = "نام نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string NationalCode { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        public string? PhoneNumber { get; set; }

        public int Status { get; set; } = 1;
        public string? UserName { get; set; }
    }

    public class StudentFilterViewModel
    {
        public string? Search { get; set; }
        public int? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}