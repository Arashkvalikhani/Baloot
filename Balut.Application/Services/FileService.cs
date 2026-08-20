using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class FileService : IFileService
    {
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", ".mp4", ".pptx", ".xlsx" };

        private const long MaxSize = 50 * 1024 * 1024; // 50MB

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentUserService _currentUser;

        public FileService(ApplicationDbContext context, IWebHostEnvironment env, ICurrentUserService currentUser)
        {
            _context = context;
            _env = env;
            _currentUser = currentUser;
        }

        public async Task<AjaxResult> SaveAsync(IFormFile file, int entityId, string entityType)
        {
            if (file == null || file.Length == 0)
                return new AjaxResult { Success = false, Message = "فایلی انتخاب نشده است." };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return new AjaxResult { Success = false, Message = "فرمت فایل مجاز نیست." };

            if (file.Length > MaxSize)
                return new AjaxResult { Success = false, Message = "حجم فایل بیشتر از 50 مگابایت است." };

            var secureName = Guid.NewGuid().ToString("N") + ext;
            var dir = Path.Combine(_env.WebRootPath, "uploads", entityType);
            Directory.CreateDirectory(dir);

            var physicalPath = Path.Combine(dir, secureName);
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var entity = new StoredFile
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{entityType}/{secureName}",
                ContentType = file.ContentType ?? "application/octet-stream",
                Size = file.Length,
                EntityId = entityId,
                EntityType = entityType,
                UploadedById = _currentUser.UserId
            };

            _context.Files.Add(entity);
            await _context.SaveChangesAsync();

            return new AjaxResult { Success = true, Message = "فایل با موفقیت آپلود شد.", Data = new { id = entity.Id } };
        }

        public async Task<FileViewModel?> GetForDownloadAsync(int fileId)
        {
            var file = await _context.Files.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null) return null;

            var http = _currentUser;
            var userId = http.UserId;
            if (userId == null) return null;

            // کارکنان دسترسی کامل دارند
            var isStaff = await _context.Users.AsNoTracking()
                .AnyAsync(u => u.Id == userId && u.Status == 1);

            bool allowed = false;

            if (file.EntityType == "Exercise")
            {
                var exercise = await _context.Exercises.AsNoTracking()
                    .Include(e => e.Session).ThenInclude(s => s.Class)
                    .FirstOrDefaultAsync(e => e.Id == file.EntityId);

                if (exercise != null)
                {
                    var teacherId = exercise.Session!.Class!.TeacherId;
                    var isTeacher = await _context.Teachers.AsNoTracking().AnyAsync(t => t.Id == teacherId && t.UserId == userId);
                    var studentId = await _context.Students.AsNoTracking().Where(s => s.UserId == userId).Select(s => (int?)s.Id).FirstOrDefaultAsync();
                    var isEnrolled = studentId != null && await _context.Enrollments.AsNoTracking()
                        .AnyAsync(e => e.ClassId == exercise.Session.ClassId && e.StudentId == studentId && e.Status == 1);
                    var parentId = await _context.Parents.AsNoTracking().Where(p => p.UserId == userId).Select(p => (int?)p.Id).FirstOrDefaultAsync();
                    var isParent = parentId != null && await _context.Parents.AsNoTracking()
                        .AnyAsync(p => p.Id == parentId && p.Students.Any(s => s.Enrollments.Any(e => e.ClassId == exercise.Session.ClassId && e.Status == 1)));

                    allowed = isTeacher || isEnrolled || isParent;
                }
            }
            else if (file.EntityType == "Submission")
            {
                var submission = await _context.ExerciseSubmissions.AsNoTracking()
                    .Include(s => s.Exercise).ThenInclude(e => e.Session).ThenInclude(s => s.Class)
                    .FirstOrDefaultAsync(s => s.Id == file.EntityId);

                if (submission != null)
                {
                    var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == submission.StudentId);
                    var isOwner = student?.UserId == userId;
                    var teacherId = submission.Exercise!.Session!.Class!.TeacherId;
                    var isTeacher = await _context.Teachers.AsNoTracking().AnyAsync(t => t.Id == teacherId && t.UserId == userId);
                    var isParentOfStudent = student != null && await _context.Parents.AsNoTracking()
                        .AnyAsync(p => p.UserId == userId && p.Students.Any(s => s.Id == student.Id));

                    allowed = isOwner || isTeacher || isParentOfStudent;
                }
            }

            if (!allowed && !isStaff) return null;

            return new FileViewModel
            {
                Id = file.Id,
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                Size = file.Size,
                EntityId = file.EntityId,
                EntityType = file.EntityType
            };
        }
    }
}