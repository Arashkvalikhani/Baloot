using Balut.Application.Interfaces;
using Balut.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Balut.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

        public async Task<int?> GetTeacherIdAsync()
        {
            var userId = UserId;
            if (userId == null) return null;
            return await _context.Teachers.AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(t => (int?)t.Id).FirstOrDefaultAsync();
        }

        public async Task<int?> GetStudentIdAsync()
        {
            var userId = UserId;
            if (userId == null) return null;
            return await _context.Students.AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync();
        }

        public async Task<int?> GetParentIdAsync()
        {
            var userId = UserId;
            if (userId == null) return null;
            return await _context.Parents.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => (int?)p.Id).FirstOrDefaultAsync();
        }
    }
}