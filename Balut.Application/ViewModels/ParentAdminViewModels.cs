using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class ParentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string NationalCode { get; set; } = string.Empty;

        public string? Occupation { get; set; }
        public string? PhoneNumber { get; set; }
        public int Status { get; set; } = 1;
    }

    public class ParentDetailViewModel : ParentViewModel
    {
        public List<ChildItemViewModel> Children { get; set; } = new();
    }

    public class ChildItemViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NationalCode { get; set; }
    }

    public class AddChildRequest
    {
        public int ParentId { get; set; }
        public int StudentId { get; set; }
    }

    public class RemoveChildRequest
    {
        public int ParentId { get; set; }
        public int StudentId { get; set; }
    }
}