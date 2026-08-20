using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public AttendanceService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<List<AttendanceEditViewModel>> GetSessionAttendanceAsync(int sessionId)
        {
            var classId = await _context.Sessions.AsNoTracking()
                .Where(s => s.Id == sessionId).Select(s => s.ClassId).FirstOrDefaultAsync();

            var students = await _context.Enrollments.AsNoTracking()
                .Where(e => e.ClassId == classId && e.Status == 1)
                .Select(e => new { e.StudentId, Name = e.Student!.User!.FirstName + " " + e.Student.User.LastName })
                .ToListAsync();

            var attendances = await _context.Attendances.AsNoTracking()
                .Where(a => a.SessionId == sessionId)
                .ToDictionaryAsync(a => a.StudentId);

            return students.Select(s => new AttendanceEditViewModel
            {
                StudentId = s.StudentId,
                StudentName = s.Name,
                Status = attendances.TryGetValue(s.StudentId, out var a) ? a.Status : 1,
                LateMinutes = attendances.TryGetValue(s.StudentId, out var a2) ? a2.LateMinutes : null
            }).ToList();
        }

        public async Task<AjaxResult> SaveAsync(int sessionId, List<AttendanceEditViewModel> items)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
                return new AjaxResult { Success = false, Message = "جلسه یافت نشد." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in items)
                {
                    if (item.Status is < 1 or > 3) continue;
                    if (item.Status != 3) item.LateMinutes = null;

                    var existing = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.StudentId == item.StudentId);

                    if (existing == null)
                    {
                        _context.Attendances.Add(new Attendance
                        {
                            SessionId = sessionId,
                            StudentId = item.StudentId,
                            Status = item.Status,
                            LateMinutes = item.LateMinutes
                        });
                    }
                    else
                    {
                        existing.Status = item.Status;
                        existing.LateMinutes = item.LateMinutes;
                    }

                    if (item.Status == 2)
                    {
                        var userId = await _context.Students.AsNoTracking()
                            .Where(s => s.Id == item.StudentId).Select(s => s.UserId).FirstOrDefaultAsync();

                        if (userId != null)
                        {
                            _context.Notifications.Add(new Notification
                            {
                                UserId = userId,
                                Title = "غیبت در جلسه",
                                Message = $"شما در جلسه {session.SessionNumber} غایب بوده‌اید.",
                                Type = 4,
                                IsRead = false
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLog.LogAsync("Save", "Attendance", sessionId.ToString(), "ثبت حضور و غیاب جلسه");

                return new AjaxResult { Success = true, Message = "حضور و غیاب با موفقیت ثبت شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }

        public async Task<List<AttendanceReportViewModel>> GetReportAsync(int teacherId, int? classId, int? sessionId)
        {
            var query = _context.Attendances.AsNoTracking()
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Session).ThenInclude(s => s.Class)
                .Where(a => a.Session!.Class!.TeacherId == teacherId);

            if (classId.HasValue)
                query = query.Where(a => a.Session!.ClassId == classId.Value);

            if (sessionId.HasValue)
                query = query.Where(a => a.SessionId == sessionId.Value);

            return await query
                .OrderByDescending(a => a.Session!.Date)
                .ThenBy(a => a.Session!.SessionNumber)
                .ThenBy(a => a.Id)
                .Select(a => new AttendanceReportViewModel
                {
                    Id = a.Id,
                    SessionId = a.SessionId,
                    SessionNumber = a.Session!.SessionNumber,
                    ClassId = a.Session.ClassId,
                    ClassName = a.Session.Class!.Name,
                    StudentName = a.Student!.User!.FirstName + " " + a.Student.User.LastName,
                    Status = a.Status,
                    LateMinutes = a.LateMinutes,
                    Date = a.Session.Date
                }).ToListAsync();
        }

        public async Task<List<AttendanceReportViewModel>> GetByStudentAsync(int studentId)
        {
            return await _context.Attendances.AsNoTracking()
                .Include(a => a.Session).ThenInclude(s => s.Class)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Session!.Date)
                .Select(a => new AttendanceReportViewModel
                {
                    Id = a.Id,
                    SessionId = a.SessionId,
                    SessionNumber = a.Session!.SessionNumber,
                    ClassId = a.Session.ClassId,
                    ClassName = a.Session.Class!.Name,
                    StudentName = string.Empty,
                    Status = a.Status,
                    LateMinutes = a.LateMinutes,
                    Date = a.Session.Date
                }).ToListAsync();
        }
    }
}