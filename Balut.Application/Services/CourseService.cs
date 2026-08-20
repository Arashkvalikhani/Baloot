using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public CourseService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<CourseViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Courses.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c => c.Title.Contains(s) || (c.Level != null && c.Level.Contains(s)));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Capacity = c.Capacity,
                    Duration = c.Duration,
                    NumberOfSessions = c.NumberOfSessions,
                    Price = c.Price,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Level = c.Level,
                    Status = c.Status
                }).ToListAsync();

            return new PagedResult<CourseViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<CourseViewModel?> GetByIdAsync(int id)
        {
            return await _context.Courses.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Capacity = c.Capacity,
                    Duration = c.Duration,
                    NumberOfSessions = c.NumberOfSessions,
                    Price = c.Price,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Level = c.Level,
                    Status = c.Status
                }).FirstOrDefaultAsync();
        }

        public async Task<List<CourseViewModel>> GetAllAsync()
        {
            return await _context.Courses.AsNoTracking()
                .Where(c => c.Status == 1)
                .Select(c => new CourseViewModel { Id = c.Id, Title = c.Title })
                .ToListAsync();
        }

        public async Task<AjaxResult> CreateAsync(CourseViewModel model)
        {
            if (model.EndDate <= model.StartDate)
                return new AjaxResult { Success = false, Message = "تاریخ پایان باید بعد از تاریخ شروع باشد." };

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                Capacity = model.Capacity,
                Duration = model.Duration,
                NumberOfSessions = model.NumberOfSessions,
                Price = model.Price,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Level = model.Level,
                Status = 1
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Course", course.Id.ToString(), $"دوره {model.Title} ایجاد شد");
            return new AjaxResult { Success = true, Message = "دوره با موفقیت ثبت شد." };
        }

        public async Task<AjaxResult> UpdateAsync(CourseViewModel model)
        {
            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null) return new AjaxResult { Success = false, Message = "دوره یافت نشد." };

            if (model.EndDate <= model.StartDate)
                return new AjaxResult { Success = false, Message = "تاریخ پایان باید بعد از تاریخ شروع باشد." };

            course.Title = model.Title;
            course.Description = model.Description;
            course.Capacity = model.Capacity;
            course.Duration = model.Duration;
            course.NumberOfSessions = model.NumberOfSessions;
            course.Price = model.Price;
            course.StartDate = model.StartDate;
            course.EndDate = model.EndDate;
            course.Level = model.Level;
            course.Status = model.Status;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Course", course.Id.ToString(), "ویرایش دوره");
            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return new AjaxResult { Success = false, Message = "دوره یافت نشد." };

            course.Status = 0;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Course", id.ToString(), "غیرفعال‌سازی دوره");
            return new AjaxResult { Success = true, Message = "دوره حذف شد." };
        }
    }
}