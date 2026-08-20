using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Balut.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseSubmission> ExerciseSubmissions { get; set; }
        public DbSet<StoredFile> Files { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");

            builder.Entity<StoredFile>().ToTable("Files");

            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Parent>()
                .HasOne(p => p.User)
                .WithOne(u => u.Parent)
                .HasForeignKey<Parent>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Parent>()
                .HasMany(p => p.Students)
                .WithMany(s => s.Parents)
                .UsingEntity<Dictionary<string, object>>(
                    "ParentStudent",
                    j => j.HasOne<Student>().WithMany().HasForeignKey("StudentId").OnDelete(DeleteBehavior.NoAction),
                    j => j.HasOne<Parent>().WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Cascade));

            builder.Entity<Attendance>().ToTable("Attendance");

            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.ClassId })
                .IsUnique();

            builder.Entity<Session>()
                .HasIndex(s => new { s.ClassId, s.SessionNumber })
                .IsUnique();

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.SessionId, a.StudentId })
                .IsUnique();

            builder.Entity<Score>()
                .HasIndex(s => new { s.SessionId, s.StudentId })
                .IsUnique();

            builder.Entity<ExerciseSubmission>()
                .HasIndex(e => new { e.ExerciseId, e.StudentId })
                .IsUnique();

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}