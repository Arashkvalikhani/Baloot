using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public EnrollmentService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<EnrollmentViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Enrollments.AsNoTracking()
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(e => e.Student!.User!.FirstName.Contains(s) || e.Student.User.LastName.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FirstName + " " + e.Student.User.LastName,
                    ClassId = e.ClassId,
                    ClassName = e.Class!.Name,
                    CourseTitle = e.Class.Course!.Title,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    PaymentStatus = e.PaymentStatus
                }).ToListAsync();

            return new PagedResult<EnrollmentViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<List<EnrollmentViewModel>> GetByClassAsync(int classId)
        {
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.ClassId == classId)
                .OrderByDescending(e => e.Id)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FirstName + " " + e.Student.User.LastName,
                    ClassId = e.ClassId,
                    ClassName = e.Class!.Name,
                    CourseTitle = e.Class.Course!.Title,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    PaymentStatus = e.PaymentStatus
                }).ToListAsync();
        }

        public async Task<List<EnrollmentViewModel>> GetByStudentAsync(int studentId)
        {
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.Id)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FirstName + " " + e.Student.User.LastName,
                    ClassId = e.ClassId,
                    ClassName = e.Class!.Name,
                    CourseTitle = e.Class.Course!.Title,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    PaymentStatus = e.PaymentStatus
                }).ToListAsync();
        }

        public async Task<AjaxResult> CreateAsync(int studentId, int classId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null || student.Status != 1)
                return new AjaxResult { Success = false, Message = "دانشجوی معتبر انتخاب نشده است." };

            var cls = await _context.Classes.FindAsync(classId);
            if (cls == null || cls.Status != 1)
                return new AjaxResult { Success = false, Message = "کلاس معتبر انتخاب نشده است." };

            if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassId == classId && e.Status != 3))
                return new AjaxResult { Success = false, Message = "این دانشجو قبلاً در این کلاس ثبت‌نام شده است." };

            var activeCount = await _context.Enrollments.CountAsync(e => e.ClassId == classId && e.Status == 1);
            if (activeCount >= cls.Capacity)
                return new AjaxResult { Success = false, Message = $"ظرفیت کلاس تکمیل است ({cls.Capacity} نفر)." };

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = classId,
                EnrollmentDate = DateTime.Now,
                Status = 1,
                PaymentStatus = 0
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Enrollment", enrollment.Id.ToString(), $"ثبت‌نام دانشجو {studentId} در کلاس {classId}");

            return new AjaxResult { Success = true, Message = "ثبت‌نام با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> UpdateAsync(EnrollmentUpdateViewModel model)
        {
            var enrollment = await _context.Enrollments.FindAsync(model.Id);
            if (enrollment == null)
                return new AjaxResult { Success = false, Message = "ثبت‌نام یافت نشد." };

            if (model.Status is < 1 or > 3)
                return new AjaxResult { Success = false, Message = "وضعیت ثبت‌نام نامعتبر است." };

            if (model.PaymentStatus is < 0 or > 1)
                return new AjaxResult { Success = false, Message = "وضعیت پرداخت نامعتبر است." };

            enrollment.Status = model.Status;
            enrollment.PaymentStatus = model.PaymentStatus;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Update", "Enrollment", enrollment.Id.ToString(),
                $"ویرایش ثبت‌نام | وضعیت: {model.Status} | پرداخت: {model.PaymentStatus}");

            return new AjaxResult { Success = true, Message = "ویرایش با موفقیت انجام شد." };
        }

        public async Task<AjaxResult> DropAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return new AjaxResult { Success = false, Message = "ثبت‌نام یافت نشد." };

            enrollment.Status = 3;
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Drop", "Enrollment", id.ToString(), "انصراف از کلاس");

            return new AjaxResult { Success = true, Message = "دانشجو از کلاس حذف شد." };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return new AjaxResult { Success = false, Message = "ثبت‌نام یافت نشد." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payments = await _context.Payments.Where(p => p.EnrollmentId == id).ToListAsync();
                _context.Payments.RemoveRange(payments);

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditLog.LogAsync("Delete", "Enrollment", id.ToString(), "حذف کامل ثبت‌نام");

                return new AjaxResult { Success = true, Message = "ثبت‌نام با موفقیت حذف شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }
    }
}