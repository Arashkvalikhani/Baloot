namespace Balut.Application.ViewModels
{
    public class ParentChildViewModel
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? NationalCode { get; set; }
        public int ActiveCoursesCount { get; set; }
    }
}