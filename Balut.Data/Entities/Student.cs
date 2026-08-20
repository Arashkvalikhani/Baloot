namespace Balut.Data.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? NationalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; } = 1;

        public ApplicationUser? User { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Parent> Parents { get; set; } = new List<Parent>();
    }
}