using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public MessageService(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<PartnerViewModel>> GetPartnersAsync()
        {
            var me = _currentUser.UserId;
            if (me == null) return new List<PartnerViewModel>();

            List<PartnerViewModel> list;

            // ===== والد: معلم‌های فرزندانم =====
            var parentId = await _currentUser.GetParentIdAsync();
            if (parentId != null)
            {
                list = await _context.Parents.AsNoTracking()
                    .Where(p => p.Id == parentId)
                    .SelectMany(p => p.Students)
                    .SelectMany(s => s.Enrollments)
                    .Where(e => e.Status == 1 && e.Class!.Status == 1 && e.Class.Teacher!.UserId != null)
                    .Select(e => new PartnerViewModel
                    {
                        UserId = e.Class!.Teacher!.UserId!,
                        FullName = e.Class.Teacher.User!.FirstName + " " + e.Class.Teacher.User.LastName,
                        StudentId = e.StudentId,
                        StudentName = e.Student!.User!.FirstName + " " + e.Student.User.LastName
                    })
                    .ToListAsync();
            }
            else
            {
                // ===== معلم: والدین دانشجوهای کلاس‌هایم =====
                var teacherId = await _currentUser.GetTeacherIdAsync();
                if (teacherId == null) return new List<PartnerViewModel>();

                list = await _context.Enrollments.AsNoTracking()
                    .Where(e => e.Class!.TeacherId == teacherId && e.Status == 1 && e.Class.Status == 1)
                    .SelectMany(e => e.Student!.Parents, (e, p) => new PartnerViewModel
                    {
                        UserId = p.UserId!,
                        FullName = p.User!.FirstName + " " + p.User.LastName,
                        StudentId = e.StudentId,
                        StudentName = e.Student.User!.FirstName + " " + e.Student.User.LastName
                    })
                    .ToListAsync();
            }

            list = list.DistinctBy(x => new { x.UserId, x.StudentId }).ToList();

            // ===== شمارش پیام‌های خوانده‌نشده (IsRead = false) برای هر مخاطب =====
            var unread = await _context.Messages.AsNoTracking()
                .Where(m => m.ReceiverId == me && !m.IsRead)
                .GroupBy(m => new { m.SenderId, m.StudentId })
                .Select(g => new { g.Key.SenderId, g.Key.StudentId, Count = g.Count() })
                .ToListAsync();

            foreach (var p in list)
            {
                var u = unread.FirstOrDefault(x => x.SenderId == p.UserId && x.StudentId == p.StudentId);
                p.UnreadCount = u != null ? u.Count : 0;
            }

            return list;
        }

        public async Task<List<MessageViewModel>> GetThreadAsync(string otherUserId, int studentId)
        {
            var me = _currentUser.UserId;
            if (me == null) return new List<MessageViewModel>();

            var messages = await _context.Messages.AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.StudentId == studentId &&
                    ((m.SenderId == me && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == me)))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender!.FirstName + " " + m.Sender.LastName,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            foreach (var m in messages) m.IsMine = m.SenderId == me;

            // علامت‌گذاری پیام‌های دریافتی به عنوان خوانده‌شده
            var incoming = await _context.Messages
                .Where(m => m.StudentId == studentId && m.SenderId == otherUserId && m.ReceiverId == me && !m.IsRead)
                .ToListAsync();

            if (incoming.Count > 0)
            {
                foreach (var m in incoming) m.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return messages;
        }

        public async Task<AjaxResult> SendAsync(SendMessageRequest request)
        {
            var me = _currentUser.UserId;
            if (me == null) return new AjaxResult { Success = false, Message = "کاربر یافت نشد." };

            if (string.IsNullOrWhiteSpace(request.Content))
                return new AjaxResult { Success = false, Message = "متن پیام خالی است." };

            if (request.Content.Length > 2000)
                return new AjaxResult { Success = false, Message = "پیام خیلی طولانی است." };

            bool allowed = false;

            var parentId = await _currentUser.GetParentIdAsync();
            if (parentId != null)
            {
                var isMyChild = await _context.Parents.AsNoTracking()
                    .AnyAsync(p => p.Id == parentId && p.Students.Any(s => s.Id == request.StudentId));

                if (isMyChild)
                {
                    allowed = await _context.Enrollments.AsNoTracking()
                        .AnyAsync(e => e.StudentId == request.StudentId && e.Status == 1 &&
                                       e.Class!.Teacher!.UserId == request.ReceiverId);
                }
            }
            else
            {
                var teacherId = await _currentUser.GetTeacherIdAsync();
                if (teacherId != null)
                {
                    var inMyClass = await _context.Enrollments.AsNoTracking()
                        .AnyAsync(e => e.StudentId == request.StudentId && e.Status == 1 &&
                                       e.Class!.TeacherId == teacherId);

                    if (inMyClass)
                    {
                        allowed = await _context.Parents.AsNoTracking()
                            .AnyAsync(p => p.UserId == request.ReceiverId &&
                                           p.Students.Any(s => s.Id == request.StudentId));
                    }
                }
            }

            if (!allowed)
                return new AjaxResult { Success = false, Message = "شما مجاز به ارسال پیام به این کاربر نیستید." };

            var studentName = await _context.Students.AsNoTracking()
                .Where(s => s.Id == request.StudentId)
                .Select(s => s.User!.FirstName + " " + s.User.LastName)
                .FirstOrDefaultAsync() ?? "دانشجو";

            var message = new Message
            {
                SenderId = me,
                ReceiverId = request.ReceiverId,
                StudentId = request.StudentId,
                Content = request.Content.Trim(),
                IsRead = false
            };
            _context.Messages.Add(message);

            _context.Notifications.Add(new Notification
            {
                UserId = request.ReceiverId,
                Title = "پیام جدید",
                Message = $"پیام جدیدی درباره {studentName} دریافت کردید.",
                Type = 5,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            return new AjaxResult { Success = true, Message = "پیام ارسال شد." };
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var me = _currentUser.UserId;
            if (me == null) return 0;
            return await _context.Messages.CountAsync(m => m.ReceiverId == me && !m.IsRead);
        }
    }
}