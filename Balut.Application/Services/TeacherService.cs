using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;

        public TeacherService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditLogService auditLog)
        {
            _context = context;
            _userManager = userManager;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<TeacherViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Teachers.Include(t => t.User).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.User!.FirstName.Contains(s) || t.User!.LastName.Contains(s)
                    || t.NationalCode!.Contains(s) || (t.Expertise != null && t.Expertise.Contains(s)));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(t => t.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.User!.FirstName,
                    LastName = t.User.LastName,
                    NationalCode = t.NationalCode!,
                    Expertise = t.Expertise,
                    Bio = t.Bio,
                    PhoneNumber = t.User.PhoneNumber,
                    Status = t.Status
                }).ToListAsync();

            return new PagedResult<TeacherViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<TeacherViewModel?> GetByIdAsync(int id)
        {
            return await _context.Teachers.Include(t => t.User).AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.User!.FirstName,
                    LastName = t.User.LastName,
                    NationalCode = t.NationalCode!,
                    Expertise = t.Expertise,
                    Bio = t.Bio,
                    PhoneNumber = t.User.PhoneNumber,
                    Status = t.Status
                }).FirstOrDefaultAsync();
        }

        public async Task<List<TeacherViewModel>> GetAllAsync()
        {
            return await _context.Teachers.Include(t => t.User).AsNoTracking()
                .Where(t => t.Status == 1)
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.User!.FirstName,
                    LastName = t.User.LastName,
                    NationalCode = t.NationalCode!
                }).ToListAsync();
        }

        public async Task<AjaxResult> CreateAsync(TeacherViewModel model)
        {
            if (await _context.Teachers.AnyAsync(t => t.NationalCode == model.NationalCode))
                return new AjaxResult { Success = false, Message = "معلمی با این کد ملی قبلاً ثبت شده است." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.NationalCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true,
                    Status = 1
                };

                var result = await _userManager.CreateAsync(user, "Balut@123456");
                if (!result.Succeeded)
                    return new AjaxResult { Success = false, Message = "خطا در ایجاد کاربر: " + string.Join(", ", result.Errors.Select(e => e.Description)) };

                await _userManager.AddToRoleAsync(user, "Teacher");

                var teacher = new Teacher
                {
                    UserId = user.Id,
                    NationalCode = model.NationalCode,
                    Expertise = model.Expertise,
                    Bio = model.Bio,
                    Status = 1
                };

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditLog.LogAsync("Create", "Teacher", teacher.Id.ToString(), $"معلم {model.FirstName} {model.LastName} ثبت شد");

                return new AjaxResult { Success = true, Message = "معلم با موفقیت ثبت شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }

        public async Task<AjaxResult> UpdateAsync(TeacherViewModel model)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == model.Id);
            if (teacher == null) return new AjaxResult { Success = false, Message = "معلم یافت نشد." };

            if (await _context.Teachers.AnyAsync(t => t.NationalCode == model.NationalCode && t.Id != model.Id))
                return new AjaxResult { Success = false, Message = "کد ملی تکراری است." };

            teacher.NationalCode = model.NationalCode;
            teacher.Expertise = model.Expertise;
            teacher.Bio = model.Bio;
            teacher.Status = model.Status;

            if (teacher.User != null)
            {
                teacher.User.FirstName = model.FirstName;
                teacher.User.LastName = model.LastName;
                teacher.User.PhoneNumber = model.PhoneNumber;
                teacher.User.UserName = model.NationalCode;
                teacher.User.NormalizedUserName = model.NationalCode.ToUpper();
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Teacher", teacher.Id.ToString(), "ویرایش اطلاعات معلم");
            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return new AjaxResult { Success = false, Message = "معلم یافت نشد." };

            teacher.Status = 0;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Teacher", id.ToString(), "غیرفعال‌سازی معلم");
            return new AjaxResult { Success = true, Message = "معلم حذف شد." };
        }
    }
}