namespace Balut.Application.ViewModels
{
    public class EnrollmentViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public int Status { get; set; }
        public int PaymentStatus { get; set; }
    }

    public class EnrollmentUpdateViewModel
    {
        public int Id { get; set; }
        public int Status { get; set; }        // 1: فعال، 3: انصراف
        public int PaymentStatus { get; set; } // 0: پرداخت نشده، 1: پرداخت شده
    }
}