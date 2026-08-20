using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var now = DateTime.Now;
            var oneWeekFromNow = now.AddDays(7);

            return new AdminDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(s => s.Status == 1),
                TotalParents = await _context.Parents.CountAsync(p => p.Status == 1),
                TotalTeachers = await _context.Teachers.CountAsync(t => t.Status == 1),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalSessions = await _context.Sessions.CountAsync(),
                ActiveCourses = await _context.Courses.CountAsync(c => c.Status == 1 && c.EndDate >= now),
                EndingSoonCourses = await _context.Courses.CountAsync(c => c.Status == 1 && c.EndDate <= oneWeekFromNow),
                TotalAbsences = await _context.Attendances.CountAsync(a => a.Status == 2),
                AverageScore = await _context.Scores.AnyAsync()
                    ? Math.Round(await _context.Scores.AverageAsync(s => s.ScoreValue), 2)
                    : 0,
                PendingSubmissions = await _context.ExerciseSubmissions.CountAsync(s => s.Status == 1),
                UnreadMessages = await _context.Messages.CountAsync(m => !m.IsRead)
            };
        }
    }
}