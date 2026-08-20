namespace Balut.Data.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? NationalCode { get; set; }

        public string? Expertise { get; set; }

        public string? Bio { get; set; }

        public int Status { get; set; } = 1;

        public ApplicationUser? User { get; set; }

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}