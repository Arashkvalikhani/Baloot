using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLog;

        public ExerciseService(ApplicationDbContext context, IFileService fileService, IAuditLogService auditLog)
        {
            _context = context;
            _fileService = fileService;
            _auditLog = auditLog;
        }

        public async Task<List<ExerciseViewModel>> GetBySessionAsync(int sessionId)
        {
            return await _context.Exercises.AsNoTracking()
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.Id)
                .Select(e => new ExerciseViewModel
                {
                    Id = e.Id,
                    SessionId = e.SessionId,
                    Title = e.Title,
                    Description = e.Description,
                    Deadline = e.Deadline,
                    Status = e.Status,
                    AttachmentId = _context.Files.Where(f => f.EntityId == e.Id && f.EntityType == "Exercise").Select(f => (int?)f.Id).FirstOrDefault(),
                    AttachmentName = _context.Files.Where(f => f.EntityId == e.Id && f.EntityType == "Exercise").Select(f => f.FileName).FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<AjaxResult> CreateAsync(CreateExerciseRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return new AjaxResult { Success = false, Message = "عنوان تمرین الزامی است." };

            var session = await _context.Sessions.FindAsync(model.SessionId);
            if (session == null)
                return new AjaxResult { Success = false, Message = "جلسه یافت نشد." };

            var exercise = new Exercise
            {
                SessionId = model.SessionId,
                Title = model.Title.Trim(),
                Description = model.Description,
                Deadline = model.Deadline,
                Status = 1
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // اطلاع‌رسانی به همه دانشجویان کلاس
            var userIds = await _context.Enrollments.AsNoTracking()
                .Where(e => e.ClassId == session.ClassId && e.Status == 1 && e.Student!.UserId != null)
                .Select(e => e.Student!.UserId!)
                .ToListAsync();

            foreach (var uid in userIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = uid,
                    Title = "تمرین جدید",
                    Message = $"تمرین «{exercise.Title}» برای جلسه {session.SessionNumber} ثبت شد.",
                    Type = 6,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Create", "Exercise", exercise.Id.ToString(), $"تمرین {exercise.Title} ایجاد شد");

            return new AjaxResult { Success = true, Message = "تمرین با موفقیت ثبت شد.", Data = new { id = exercise.Id } };
        }

        public async Task<AjaxResult> DeleteAsync(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
                return new AjaxResult { Success = false, Message = "تمرین یافت نشد." };

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Delete", "Exercise", id.ToString(), "حذف تمرین");

            return new AjaxResult { Success = true, Message = "تمرین حذف شد." };
        }

        public async Task<int?> GetTeacherIdByExerciseAsync(int exerciseId)
        {
            return await _context.Exercises.AsNoTracking()
                .Where(e => e.Id == exerciseId)
                .Select(e => (int?)e.Session!.Class!.TeacherId).FirstOrDefaultAsync();
        }

        public async Task<int?> GetTeacherIdBySubmissionAsync(int submissionId)
        {
            return await _context.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.Id == submissionId)
                .Select(s => (int?)s.Exercise!.Session!.Class!.TeacherId).FirstOrDefaultAsync();
        }

        public async Task<List<SubmissionViewModel>> GetSubmissionsAsync(int exerciseId)
        {
            return await _context.ExerciseSubmissions.AsNoTracking()
                .Include(s => s.Student).ThenInclude(s => s.User)
                .Where(s => s.ExerciseId == exerciseId)
                .OrderBy(s => s.SubmittedAt)
                .Select(s => new SubmissionViewModel
                {
                    Id = s.Id,
                    ExerciseId = s.ExerciseId,
                    StudentId = s.StudentId,
                    StudentName = s.Student!.User!.FirstName + " " + s.Student.User.LastName,
                    TextContent = s.TextContent,
                    Status = s.Status,
                    SubmittedAt = s.SubmittedAt,
                    Score = s.Score,
                    TeacherComment = s.TeacherComment,
                    AttachmentId = _context.Files.Where(f => f.EntityId == s.Id && f.EntityType == "Submission").Select(f => (int?)f.Id).FirstOrDefault(),
                    AttachmentName = _context.Files.Where(f => f.EntityId == s.Id && f.EntityType == "Submission").Select(f => f.FileName).FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<AjaxResult> SubmitAsync(int exerciseId, int studentId, string? text, IFormFile? file)
        {
            var exercise = await _context.Exercises.FindAsync(exerciseId);
            if (exercise == null)
                return new AjaxResult { Success = false, Message = "تمرین یافت نشد." };

            // کلاسِ جلسه‌ی تمرین
            var classId = await _context.Sessions.AsNoTracking()
                .Where(s => s.Id == exercise.SessionId)
                .Select(s => (int?)s.ClassId)
                .FirstOrDefaultAsync();

            if (classId == null)
                return new AjaxResult { Success = false, Message = "جلسه تمرین یافت نشد." };

            // دانشجو باید در همان کلاس ثبت‌نام فعال داشته باشد
            var enrolled = await _context.Enrollments.AsNoTracking()
                .AnyAsync(e => e.StudentId == studentId && e.ClassId == classId.Value && e.Status == 1);

            if (!enrolled)
                return new AjaxResult { Success = false, Message = "شما در این کلاس ثبت‌نام نکرده‌اید." };

            if (await _context.ExerciseSubmissions.AnyAsync(s => s.ExerciseId == exerciseId && s.StudentId == studentId))
                return new AjaxResult { Success = false, Message = "شما قبلاً پاسخ این تمرین را ارسال کرده‌اید." };

            if (string.IsNullOrWhiteSpace(text) && file == null)
                return new AjaxResult { Success = false, Message = "متن یا فایل پاسخ الزامی است." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var submission = new ExerciseSubmission
                {
                    ExerciseId = exerciseId,
                    StudentId = studentId,
                    TextContent = text,
                    Status = 1,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.ExerciseSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                if (file != null)
                {
                    var fileResult = await _fileService.SaveAsync(file, submission.Id, "Submission");
                    if (!fileResult.Success)
                    {
                        await transaction.RollbackAsync();
                        return fileResult;
                    }
                }

                await transaction.CommitAsync();
                await _auditLog.LogAsync("Submit", "ExerciseSubmission", submission.Id.ToString(), "ارسال پاسخ تمرین");

                return new AjaxResult { Success = true, Message = "پاسخ شما با موفقیت ارسال شد." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AjaxResult { Success = false, Message = "خطا: " + ex.Message };
            }
        }

        public async Task<AjaxResult> GradeAsync(GradeSubmissionRequest model)
        {
            if (model.Score.HasValue && (model.Score.Value < 0 || model.Score.Value > 10))
                return new AjaxResult { Success = false, Message = "نمره باید بین 0 تا 10 باشد." };

            var submission = await _context.ExerciseSubmissions
                .Include(s => s.Exercise).ThenInclude(e => e.Session)
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

            if (submission == null)
                return new AjaxResult { Success = false, Message = "ارسال یافت نشد." };

            submission.Score = model.Score;
            submission.TeacherComment = model.Comment;
            submission.Status = 2;

            var studentUserId = await _context.Students.AsNoTracking()
                .Where(s => s.Id == submission.StudentId).Select(s => s.UserId).FirstOrDefaultAsync();

            if (studentUserId != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = studentUserId,
                    Title = "بررسی تمرین",
                    Message = model.Score.HasValue
                        ? $"تمرین شما نمره {model.Score.Value} دریافت کرد."
                        : "تمرین شما بررسی شد.",
                    Type = 3,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("Grade", "ExerciseSubmission", submission.Id.ToString(), "نمره‌دهی تمرین");

            return new AjaxResult { Success = true, Message = "نمره با موفقیت ثبت شد." };
        }

        public async Task<List<StudentExerciseViewModel>> GetByStudentAsync(int studentId)
        {
            return await _context.Exercises.AsNoTracking()
                .Where(ex => ex.Session!.Class!.Enrollments.Any(en => en.StudentId == studentId && en.Status == 1))
                .OrderByDescending(ex => ex.Id)
                .Select(ex => new StudentExerciseViewModel
                {
                    ExerciseId = ex.Id,
                    Title = ex.Title,
                    Description = ex.Description,
                    Deadline = ex.Deadline,
                    SessionNumber = ex.Session!.SessionNumber,
                    ClassName = ex.Session.Class!.Name,
                    ExerciseAttachmentId = _context.Files.Where(f => f.EntityId == ex.Id && f.EntityType == "Exercise").Select(f => (int?)f.Id).FirstOrDefault(),
                    ExerciseAttachmentName = _context.Files.Where(f => f.EntityId == ex.Id && f.EntityType == "Exercise").Select(f => f.FileName).FirstOrDefault(),
                    SubmissionId = ex.Submissions.Where(s => s.StudentId == studentId).Select(s => (int?)s.Id).FirstOrDefault(),
                    SubmissionText = ex.Submissions.Where(s => s.StudentId == studentId).Select(s => s.TextContent).FirstOrDefault(),
                    SubmissionStatus = ex.Submissions.Where(s => s.StudentId == studentId).Select(s => (int?)s.Status).FirstOrDefault(),
                    SubmissionScore = ex.Submissions.Where(s => s.StudentId == studentId).Select(s => s.Score).FirstOrDefault(),
                    TeacherComment = ex.Submissions.Where(s => s.StudentId == studentId).Select(s => s.TeacherComment).FirstOrDefault(),
                    SubmissionAttachmentId = _context.Files.Where(f => f.EntityId == ex.Submissions.Where(s => s.StudentId == studentId).Select(s => s.Id).FirstOrDefault() && f.EntityType == "Submission").Select(f => (int?)f.Id).FirstOrDefault(),
                    SubmissionAttachmentName = _context.Files.Where(f => f.EntityId == ex.Submissions.Where(s => s.StudentId == studentId).Select(s => s.Id).FirstOrDefault() && f.EntityType == "Submission").Select(f => f.FileName).FirstOrDefault()
                }).ToListAsync();
        }
    }
}