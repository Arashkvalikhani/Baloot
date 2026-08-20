using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public ClassService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<ClassViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c => c.Name.Contains(s) || c.Course!.Title.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    CourseId = c.CourseId,
                    TeacherId = c.TeacherId,
                    Room = c.Room,
                    Schedule = c.Schedule,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    CourseTitle = c.Course!.Title,
                    TeacherName = c.Teacher!.User!.FirstName + " " + c.Teacher.User.LastName,
                    StudentCount = c.Enrollments.Count(e => e.Status == 1)
                }).ToListAsync();

            return new PagedResult<ClassViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<ClassViewModel?> GetByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    CourseId = c.CourseId,
                    TeacherId = c.TeacherId,
                    Room = c.Room,
                    Schedule = c.Schedule,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    CourseTitle = c.Course!.Title,
                    TeacherName = c.Teacher!.User!.FirstName + " " + c.Teacher.User.LastName,
                    StudentCount = c.Enrollments.Count(e => e.Status == 1)
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ClassViewModel>> GetAllAsync()
        {
            return await _context.Classes.AsNoTracking()
                .Where(c => c.Status == 1)
                .Include(c => c.Course)
                .Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    CourseTitle = c.Course!.Title
                }).ToListAsync();
        }

        public async Task<List<ClassViewModel>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.Classes.AsNoTracking()
                .Where(c => c.TeacherId == teacherId && c.Status == 1)
                .Include(c => c.Course)
                .OrderBy(c => c.Id)
                .Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    CourseId = c.CourseId,
                    TeacherId = c.TeacherId,
                    Room = c.Room,
                    Schedule = c.Schedule,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    CourseTitle = c.Course!.Title,
                    StudentCount = c.Enrollments.Count(e => e.Status == 1)
                }).ToListAsync();
        }

        public async Task<bool> IsTeacherOfClassAsync(int classId, int teacherId)
        {
            return await _context.Classes.AsNoTracking()
                .AnyAsync(c => c.Id == classId && c.TeacherId == teacherId);
        }

        public async Task<AjaxResult> CreateAsync(ClassViewModel model)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == model.CourseId && c.Status == 1))
                return new AjaxResult { Success = false, Message = "دوره انتخاب شده معتبر نیست." };

            if (!await _context.Teachers.AnyAsync(t => t.Id == model.TeacherId && t.Status == 1))
                return new AjaxResult { Success = false, Message = "معلم انتخاب شده معتبر نیست." };

            var cls = new Class
            {
                Name = model.Name,
                CourseId = model.CourseId,
                TeacherId = model.TeacherId,
                Room = model.Room,
                Schedule = model.Schedule,
                Capacity = model.Capacity,
                Status = 1
            };

            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Class", cls.Id.ToString(), $"کلاس {model.Name} ایجاد شد");

            return new AjaxResult { Success = true, Message = "کلاس با موفقیت ثبت شد." };
        }

        public async Task<AjaxResult> UpdateAsync(ClassViewModel model)
        {
            var cls = await _context.Classes.FindAsync(model.Id);
            if (cls == null) return new AjaxResult { Success = false, Message = "کلاس یافت نشد." };

            cls.Name = model.Name;
            cls.CourseId = model.CourseId;
            cls.TeacherId = model.TeacherId;
            cls.Room = model.Room;
            cls.Schedule = model.Schedule;
            cls.Capacity = model.Capacity;
            cls.Status = model.Status;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Class", cls.Id.ToString(), "ویرایش کلاس");

            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var cls = await _context.Classes.FindAsync(id);
            if (cls == null) return new AjaxResult { Success = false, Message = "کلاس یافت نشد." };

            cls.Status = 0;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Class", id.ToString(), "غیرفعال‌سازی کلاس");

            return new AjaxResult { Success = true, Message = "کلاس حذف شد." };
        }
    }
}