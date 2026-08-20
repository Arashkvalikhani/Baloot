using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;

        public StudentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLog)
        {
            _context = context;
            _userManager = userManager;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<StudentViewModel>> GetPagedAsync(StudentFilterViewModel filter)
        {
            var query = _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .AsQueryable();

            // جستجو
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(s =>
                    (s.User!.FirstName.Contains(search)) ||
                    (s.User!.LastName.Contains(search)) ||
                    (s.NationalCode!.Contains(search)));
            }

            // فیلتر وضعیت
            if (filter.Status.HasValue)
            {
                query = query.Where(s => s.Status == filter.Status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FirstName = s.User!.FirstName,
                    LastName = s.User.LastName,
                    NationalCode = s.NationalCode!,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    PhoneNumber = s.User.PhoneNumber,
                    Status = s.Status
                })
                .ToListAsync();

            return new PagedResult<StudentViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<StudentViewModel?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FirstName = s.User!.FirstName,
                    LastName = s.User.LastName,
                    NationalCode = s.NationalCode!,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    PhoneNumber = s.User.PhoneNumber,
                    Status = s.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<StudentViewModel>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .Where(s => s.Status == 1)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FirstName = s.User!.FirstName,
                    LastName = s.User.LastName,
                    NationalCode = s.NationalCode!
                })
                .ToListAsync();
        }

        public async Task<AjaxResult> CreateAsync(StudentViewModel model)
        {
            // بررسی تکراری نبودن کد ملی
            var exists = await _context.Students
                .AnyAsync(s => s.NationalCode == model.NationalCode);

            if (exists)
            {
                return new AjaxResult
                {
                    Success = false,
                    Message = "دانشجویی با این کد ملی قبلاً ثبت شده است."
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ساخت کاربر
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
                {
                    return new AjaxResult
                    {
                        Success = false,
                        Message = "خطا در ایجاد کاربر: " + string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                await _userManager.AddToRoleAsync(user, "Student");

                // ساخت دانشجو
                var student = new Student
                {
                    UserId = user.Id,
                    NationalCode = model.NationalCode,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    Status = 1
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _auditLog.LogAsync("Create", "Student", student.Id.ToString(),
                    $"دانشجوی جدید {model.FirstName} {model.LastName} ثبت شد");

                return new AjaxResult
                {
                    Success = true,
                    Message = "دانشجو با موفقیت ثبت شد.",
                    Data = new { id = student.Id }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult
                {
                    Success = false,
                    Message = "خطا در ثبت دانشجو: " + ex.Message
                };
            }
        }

        public async Task<AjaxResult> UpdateAsync(StudentViewModel model)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (student == null)
            {
                return new AjaxResult { Success = false, Message = "دانشجو یافت نشد." };
            }

            // بررسی تکراری نبودن کد ملی
            var exists = await _context.Students
                .AnyAsync(s => s.NationalCode == model.NationalCode && s.Id != model.Id);

            if (exists)
            {
                return new AjaxResult { Success = false, Message = "کد ملی تکراری است." };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                student.NationalCode = model.NationalCode;
                student.DateOfBirth = model.DateOfBirth;
                student.Address = model.Address;
                student.Status = model.Status;

                if (student.User != null)
                {
                    student.User.FirstName = model.FirstName;
                    student.User.LastName = model.LastName;
                    student.User.PhoneNumber = model.PhoneNumber;
                    student.User.UserName = model.NationalCode;
                    student.User.NormalizedUserName = model.NationalCode.ToUpper();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditLog.LogAsync("Update", "Student", student.Id.ToString(),
                    $"اطلاعات دانشجو {model.FirstName} {model.LastName} ویرایش شد");

                return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا در ویرایش: " + ex.Message };
            }
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return new AjaxResult { Success = false, Message = "دانشجو یافت نشد." };
            }

            // به جای حذف کامل، غیرفعال می‌کنیم (Soft Delete)
            student.Status = 0;
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Delete", "Student", id.ToString(), "دانشجو غیرفعال شد");

            return new AjaxResult { Success = true, Message = "دانشجو با موفقیت حذف شد." };
        }

        public async Task<AjaxResult> ToggleStatusAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return new AjaxResult { Success = false, Message = "دانشجو یافت نشد." };
            }

            student.Status = student.Status == 1 ? 0 : 1;
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("ToggleStatus", "Student", id.ToString(),
                $"وضعیت دانشجو به {(student.Status == 1 ? "فعال" : "غیرفعال")} تغییر کرد");

            return new AjaxResult
            {
                Success = true,
                Message = student.Status == 1 ? "دانشجو فعال شد." : "دانشجو غیرفعال شد."
            };
        }
    }
}