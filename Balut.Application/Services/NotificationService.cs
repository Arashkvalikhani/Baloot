using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public NotificationService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<NotificationViewModel>> GetMyAsync()
        {
            var userId = _currentUser.UserId;
            if (userId == null) return new List<NotificationViewModel>();

            return await _context.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var userId = _currentUser.UserId;
            if (userId == null) return 0;
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<AjaxResult> MarkReadAsync(int id)
        {
            var userId = _currentUser.UserId;
            var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return new AjaxResult { Success = false, Message = "اعلان یافت نشد." };

            n.IsRead = true;
            await _context.SaveChangesAsync();
            return new AjaxResult { Success = true, Message = "اعلان خوانده شد." };
        }

        public async Task<AjaxResult> MarkAllReadAsync()
        {
            var userId = _currentUser.UserId;
            if (userId == null) return new AjaxResult { Success = false, Message = "کاربر یافت نشد." };

            var items = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in items) n.IsRead = true;
            await _context.SaveChangesAsync();
            return new AjaxResult { Success = true, Message = "همه اعلان‌ها خوانده شدند." };
        }
    }
}