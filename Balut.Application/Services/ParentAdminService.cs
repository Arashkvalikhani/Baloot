using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class ParentAdminService : IParentAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;

        public ParentAdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLog)
        {
            _context = context;
            _userManager = userManager;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<ParentViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Parents.Include(p => p.User).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(p => p.User!.FirstName.Contains(s) || p.User!.LastName.Contains(s) || p.NationalCode!.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new ParentViewModel
                {
                    Id = p.Id,
                    FirstName = p.User!.FirstName,
                    LastName = p.User.LastName,
                    NationalCode = p.NationalCode!,
                    Occupation = p.Occupation,
                    PhoneNumber = p.User.PhoneNumber,
                    Status = p.Status
                }).ToListAsync();

            return new PagedResult<ParentViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<ParentDetailViewModel?> GetDetailAsync(int id)
        {
            return await _context.Parents
                .Include(p => p.User)
                .Include(p => p.Students).ThenInclude(s => s.User)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ParentDetailViewModel
                {
                    Id = p.Id,
                    FirstName = p.User!.FirstName,
                    LastName = p.User.LastName,
                    NationalCode = p.NationalCode!,
                    Occupation = p.Occupation,
                    PhoneNumber = p.User.PhoneNumber,
                    Status = p.Status,
                    Children = p.Students.Select(s => new ChildItemViewModel
                    {
                        StudentId = s.Id,
                        FullName = s.User!.FirstName + " " + s.User.LastName,
                        NationalCode = s.NationalCode
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<AjaxResult> CreateAsync(ParentViewModel model)
        {
            if (await _context.Parents.AnyAsync(p => p.NationalCode == model.NationalCode))
                return new AjaxResult { Success = false, Message = "والدی با این کد ملی قبلاً ثبت شده است." };

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

                await _userManager.AddToRoleAsync(user, "Parent");

                var parent = new Parent
                {
                    UserId = user.Id,
                    NationalCode = model.NationalCode,
                    Occupation = model.Occupation,
                    Status = 1
                };

                _context.Parents.Add(parent);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditLog.LogAsync("Create", "Parent", parent.Id.ToString(), $"والد {model.FirstName} {model.LastName} ثبت شد");

                return new AjaxResult { Success = true, Message = "والد با موفقیت ثبت شد. رمز پیش‌فرض: Balut@123456" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }

        public async Task<AjaxResult> UpdateAsync(ParentViewModel model)
        {
            var parent = await _context.Parents.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == model.Id);
            if (parent == null) return new AjaxResult { Success = false, Message = "والد یافت نشد." };

            if (await _context.Parents.AnyAsync(p => p.NationalCode == model.NationalCode && p.Id != model.Id))
                return new AjaxResult { Success = false, Message = "کد ملی تکراری است." };

            parent.NationalCode = model.NationalCode;
            parent.Occupation = model.Occupation;
            parent.Status = model.Status;

            if (parent.User != null)
            {
                parent.User.FirstName = model.FirstName;
                parent.User.LastName = model.LastName;
                parent.User.PhoneNumber = model.PhoneNumber;
                parent.User.UserName = model.NationalCode;
                parent.User.NormalizedUserName = model.NationalCode.ToUpper();
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Parent", parent.Id.ToString(), "ویرایش اطلاعات والد");

            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> ToggleStatusAsync(int id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null) return new AjaxResult { Success = false, Message = "والد یافت نشد." };

            parent.Status = parent.Status == 1 ? 0 : 1;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("ToggleStatus", "Parent", id.ToString(), "تغییر وضعیت والد");

            return new AjaxResult { Success = true, Message = parent.Status == 1 ? "والد فعال شد." : "والد غیرفعال شد." };
        }

        public async Task<AjaxResult> AddChildAsync(int parentId, int studentId)
        {
            var parent = await _context.Parents.Include(p => p.Students).FirstOrDefaultAsync(p => p.Id == parentId);
            if (parent == null) return new AjaxResult { Success = false, Message = "والد یافت نشد." };

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return new AjaxResult { Success = false, Message = "دانشجو یافت نشد." };

            if (parent.Students.Any(s => s.Id == studentId))
                return new AjaxResult { Success = false, Message = "این فرزند قبلاً به والد متصل شده است." };

            parent.Students.Add(student);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("AddChild", "Parent", parentId.ToString(), $"اتصال فرزند {studentId} به والد {parentId}");

            return new AjaxResult { Success = true, Message = "فرزند با موفقیت متصل شد." };
        }

        public async Task<AjaxResult> RemoveChildAsync(int parentId, int studentId)
        {
            var parent = await _context.Parents.Include(p => p.Students).FirstOrDefaultAsync(p => p.Id == parentId);
            if (parent == null) return new AjaxResult { Success = false, Message = "والد یافت نشد." };

            var student = parent.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null) return new AjaxResult { Success = false, Message = "این فرزند به والد متصل نیست." };

            parent.Students.Remove(student);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("RemoveChild", "Parent", parentId.ToString(), $"حذف اتصال فرزند {studentId}");

            return new AjaxResult { Success = true, Message = "اتصال فرزند حذف شد." };
        }
    }
}