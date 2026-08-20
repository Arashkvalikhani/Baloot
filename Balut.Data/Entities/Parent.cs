namespace Balut.Data.Entities
{
    public class Parent
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? NationalCode { get; set; }
        public string? Occupation { get; set; }
        public int Status { get; set; } = 1;

        public ApplicationUser? User { get; set; }
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}