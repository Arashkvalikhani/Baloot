namespace Balut.Data.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int EnrollmentId { get; set; }
        public decimal Amount { get; set; }
        public string? RefId { get; set; }
        public string? Authority { get; set; }
        public int Status { get; set; } = 0; // 0: Pending, 1: Success, 2: Failed
        public DateTime? PaymentDate { get; set; }
        public string? Description { get; set; }

        public ApplicationUser? User { get; set; }
        public Enrollment? Enrollment { get; set; }
    }
}