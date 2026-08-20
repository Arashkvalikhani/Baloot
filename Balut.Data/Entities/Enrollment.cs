namespace Balut.Data.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public int Status { get; set; } = 1; // 1: Active, 2: Completed, 3: Dropped
        public int PaymentStatus { get; set; } = 0; // 0: Unpaid, 1: Paid

        public Student? Student { get; set; }
        public Class? Class { get; set; }
    }
}