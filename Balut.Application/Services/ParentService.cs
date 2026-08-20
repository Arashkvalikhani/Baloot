using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class ParentService : IParentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ParentService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<ParentChildViewModel>> GetMyChildrenAsync()
        {
            var userId = _currentUser.UserId;
            if (userId == null) return new List<ParentChildViewModel>();

            return await _context.Parents.AsNoTracking()
                .Where(p => p.UserId == userId)
                .SelectMany(p => p.Students)
                .Select(s => new ParentChildViewModel
                {
                    StudentId = s.Id,
                    FirstName = s.User!.FirstName,
                    LastName = s.User.LastName,
                    NationalCode = s.NationalCode,
                    ActiveCoursesCount = s.Enrollments.Count(e => e.Status == 1)
                }).ToListAsync();
        }

        public async Task<bool> IsParentOfStudentAsync(int studentId)
        {
            var userId = _currentUser.UserId;
            if (userId == null) return false;

            return await _context.Parents.AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.Students.Any(s => s.Id == studentId));
        }
    }
}