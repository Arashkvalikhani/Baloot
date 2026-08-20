using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public SessionService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<List<SessionViewModel>> GetByClassAsync(int classId)
        {
            return await _context.Sessions.AsNoTracking()
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.SessionNumber)
                .Select(s => new SessionViewModel
                {
                    Id = s.Id,
                    ClassId = s.ClassId,
                    SessionNumber = s.SessionNumber,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Topic = s.Topic,
                    Description = s.Description,
                    Status = s.Status
                }).ToListAsync();
        }

        public async Task<SessionViewModel?> GetByIdAsync(int id)
        {
            return await _context.Sessions.AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SessionViewModel
                {
                    Id = s.Id,
                    ClassId = s.ClassId,
                    SessionNumber = s.SessionNumber,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Topic = s.Topic,
                    Description = s.Description,
                    Status = s.Status
                }).FirstOrDefaultAsync();
        }

        public async Task<int?> GetClassTeacherIdAsync(int sessionId)
        {
            return await _context.Sessions.AsNoTracking()
                .Where(s => s.Id == sessionId)
                .Select(s => (int?)s.Class!.TeacherId).FirstOrDefaultAsync();
        }

        public async Task<AjaxResult> CreateAsync(SessionViewModel model)
        {
            if (!await _context.Classes.AnyAsync(c => c.Id == model.ClassId))
                return new AjaxResult { Success = false, Message = "کلاس معتبر نیست." };

            if (model.EndTime <= model.StartTime)
                return new AjaxResult { Success = false, Message = "ساعت پایان باید بعد از ساعت شروع باشد." };

            if (await _context.Sessions.AnyAsync(s => s.ClassId == model.ClassId && s.SessionNumber == model.SessionNumber))
                return new AjaxResult { Success = false, Message = $"جلسه شماره {model.SessionNumber} قبلاً برای این کلاس ثبت شده است." };

            var session = new Session
            {
                ClassId = model.ClassId,
                SessionNumber = model.SessionNumber,
                Date = model.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Topic = model.Topic,
                Description = model.Description,
                Status = model.Status
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Session", session.Id.ToString(), $"جلسه {model.SessionNumber} ایجاد شد");

            return new AjaxResult { Success = true, Message = "جلسه با موفقیت ثبت شد." };
        }

        public async Task<AjaxResult> UpdateAsync(SessionViewModel model)
        {
            var session = await _context.Sessions.FindAsync(model.Id);
            if (session == null)
                return new AjaxResult { Success = false, Message = "جلسه یافت نشد." };

            if (await _context.Sessions.AnyAsync(s => s.ClassId == session.ClassId && s.SessionNumber == model.SessionNumber && s.Id != model.Id))
                return new AjaxResult { Success = false, Message = "شماره جلسه تکراری است." };

            if (model.EndTime <= model.StartTime)
                return new AjaxResult { Success = false, Message = "ساعت پایان باید بعد از ساعت شروع باشد." };

            session.SessionNumber = model.SessionNumber;
            session.Date = model.Date;
            session.StartTime = model.StartTime;
            session.EndTime = model.EndTime;
            session.Topic = model.Topic;
            session.Description = model.Description;
            session.Status = model.Status;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Session", session.Id.ToString(), "ویرایش جلسه");

            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
                return new AjaxResult { Success = false, Message = "جلسه یافت نشد." };

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Session", id.ToString(), "حذف جلسه");

            return new AjaxResult { Success = true, Message = "جلسه حذف شد." };
        }
    }
}