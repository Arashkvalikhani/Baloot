using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class ScoreService : IScoreService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public ScoreService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<List<ScoreEditViewModel>> GetSessionScoresAsync(int sessionId)
        {
            var classId = await _context.Sessions.AsNoTracking()
                .Where(s => s.Id == sessionId).Select(s => s.ClassId).FirstOrDefaultAsync();

            var students = await _context.Enrollments.AsNoTracking()
                .Where(e => e.ClassId == classId && e.Status == 1)
                .Select(e => new { e.StudentId, Name = e.Student!.User!.FirstName + " " + e.Student.User.LastName })
                .ToListAsync();

            var scores = await _context.Scores.AsNoTracking()
                .Where(s => s.SessionId == sessionId)
                .ToDictionaryAsync(s => s.StudentId);

            return students.Select(s => new ScoreEditViewModel
            {
                StudentId = s.StudentId,
                StudentName = s.Name,
                ScoreValue = scores.TryGetValue(s.StudentId, out var sc) ? sc.ScoreValue : null,
                Comments = scores.TryGetValue(s.StudentId, out var sc2) ? sc2.Comments : null
            }).ToList();
        }

        public async Task<AjaxResult> SaveSessionScoresAsync(int sessionId, List<ScoreEditViewModel> items)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
                return new AjaxResult { Success = false, Message = "جلسه یافت نشد." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in items)
                {
                    if (!item.ScoreValue.HasValue) continue;

                    if (item.ScoreValue.Value < 0 || item.ScoreValue.Value > 10)
                        return new AjaxResult { Success = false, Message = "نمره باید بین 0 تا 10 باشد." };

                    var existing = await _context.Scores
                        .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.StudentId == item.StudentId);

                    if (existing == null)
                    {
                        _context.Scores.Add(new Score
                        {
                            SessionId = sessionId,
                            StudentId = item.StudentId,
                            ScoreValue = item.ScoreValue.Value,
                            Comments = item.Comments
                        });
                    }
                    else
                    {
                        existing.ScoreValue = item.ScoreValue.Value;
                        existing.Comments = item.Comments;
                    }

                    // اطلاع‌رسانی نمره جدید به دانشجو
                    var userId = await _context.Students.AsNoTracking()
                        .Where(s => s.Id == item.StudentId).Select(s => s.UserId).FirstOrDefaultAsync();

                    if (userId != null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = userId,
                            Title = "نمره جدید",
                            Message = $"نمره شما در جلسه {session.SessionNumber}: {item.ScoreValue.Value}",
                            Type = 3,
                            IsRead = false
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLog.LogAsync("Save", "Score", sessionId.ToString(), "ثبت نمرات جلسه");

                return new AjaxResult { Success = true, Message = "نمرات با موفقیت ثبت شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }

        public async Task<List<ScoreReportViewModel>> GetByStudentAsync(int studentId)
        {
            return await _context.Scores.AsNoTracking()
                .Include(s => s.Session).ThenInclude(se => se.Class).ThenInclude(c => c.Course)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.Session!.Date)
                .Select(s => new ScoreReportViewModel
                {
                    Id = s.Id,
                    SessionId = s.SessionId,
                    SessionNumber = s.Session!.SessionNumber,
                    ClassName = s.Session.Class!.Name,
                    CourseTitle = s.Session.Class.Course!.Title,
                    ScoreValue = s.ScoreValue,
                    Comments = s.Comments,
                    Date = s.Session.Date
                }).ToListAsync();
        }
    }
}