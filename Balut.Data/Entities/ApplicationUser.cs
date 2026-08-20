using Microsoft.AspNetCore.Identity;

namespace Balut.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Status { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Student? Student { get; set; }
        public virtual Parent? Parent { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}