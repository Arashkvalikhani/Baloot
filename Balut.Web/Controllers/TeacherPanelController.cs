using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherPanelController : Controller
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IClassService _classService;
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IScoreService _scoreService;
        private readonly IExerciseService _exerciseService;
        private readonly IFileService _fileService;

        public TeacherPanelController(
            ICurrentUserService currentUser,
            IClassService classService,
            ISessionService sessionService,
            IAttendanceService attendanceService,
            IEnrollmentService enrollmentService,
            IScoreService scoreService,
            IExerciseService exerciseService,
            IFileService fileService)
        {
            _currentUser = currentUser;
            _classService = classService;
            _sessionService = sessionService;
            _attendanceService = attendanceService;
            _enrollmentService = enrollmentService;
            _scoreService = scoreService;
            _exerciseService = exerciseService;
            _fileService = fileService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetMyClasses()
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            if (teacherId == null) return Json(new List<ClassViewModel>());
            return Json(await _classService.GetByTeacherIdAsync(teacherId.Value));
        }

        public async Task<IActionResult> Students(int classId)
        {
            if (!await IsMyClass(classId)) return Forbid();
            ViewData["ClassId"] = classId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClassStudents(int classId)
        {
            if (!await IsMyClass(classId)) return Forbid();
            return Json(await _enrollmentService.GetByClassAsync(classId));
        }

        public async Task<IActionResult> Sessions(int classId)
        {
            if (!await IsMyClass(classId)) return Forbid();
            ViewData["ClassId"] = classId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSessions(int classId)
        {
            if (!await IsMyClass(classId)) return Forbid();
            return Json(await _sessionService.GetByClassAsync(classId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession([FromBody] SessionViewModel model)
        {
            if (!await IsMyClass(model.ClassId)) return Forbid();
            return Json(await _sessionService.CreateAsync(model));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSession([FromBody] SessionViewModel model)
        {
            if (!await IsMySession(model.Id)) return Forbid();
            return Json(await _sessionService.UpdateAsync(model));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession([FromBody] IdRequest request)
        {
            if (!await IsMySession(request.Id)) return Forbid();
            return Json(await _sessionService.DeleteAsync(request.Id));
        }

        public async Task<IActionResult> Attendance(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            ViewData["SessionId"] = sessionId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceData(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            return Json(await _attendanceService.GetSessionAttendanceAsync(sessionId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance([FromBody] SaveAttendanceRequest request)
        {
            if (!await IsMySession(request.SessionId)) return Forbid();
            return Json(await _attendanceService.SaveAsync(request.SessionId, request.Items));
        }

        public async Task<IActionResult> Scores(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            ViewData["SessionId"] = sessionId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetScoresData(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            return Json(await _scoreService.GetSessionScoresAsync(sessionId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScores([FromBody] SaveScoresRequest request)
        {
            if (!await IsMySession(request.SessionId)) return Forbid();
            return Json(await _scoreService.SaveSessionScoresAsync(request.SessionId, request.Items));
        }

        // ===== تکالیف =====
        public async Task<IActionResult> Exercises(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            ViewData["SessionId"] = sessionId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetExercises(int sessionId)
        {
            if (!await IsMySession(sessionId)) return Forbid();
            return Json(await _exerciseService.GetBySessionAsync(sessionId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
        {
            if (!await IsMySession(request.SessionId)) return Forbid();
            return Json(await _exerciseService.CreateAsync(request));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExercise([FromBody] IdRequest request)
        {
            var teacherId = await _exerciseService.GetTeacherIdByExerciseAsync(request.Id);
            if (!await IsMyTeacherId(teacherId)) return Forbid();
            return Json(await _exerciseService.DeleteAsync(request.Id));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExerciseFile([FromForm] int exerciseId, IFormFile file)
        {
            var teacherId = await _exerciseService.GetTeacherIdByExerciseAsync(exerciseId);
            if (!await IsMyTeacherId(teacherId)) return Forbid();
            return Json(await _fileService.SaveAsync(file, exerciseId, "Exercise"));
        }

        public async Task<IActionResult> Submissions(int exerciseId)
        {
            var teacherId = await _exerciseService.GetTeacherIdByExerciseAsync(exerciseId);
            if (!await IsMyTeacherId(teacherId)) return Forbid();
            ViewData["ExerciseId"] = exerciseId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSubmissions(int exerciseId)
        {
            var teacherId = await _exerciseService.GetTeacherIdByExerciseAsync(exerciseId);
            if (!await IsMyTeacherId(teacherId)) return Forbid();
            return Json(await _exerciseService.GetSubmissionsAsync(exerciseId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeSubmissionRequest request)
        {
            var teacherId = await _exerciseService.GetTeacherIdBySubmissionAsync(request.SubmissionId);
            if (!await IsMyTeacherId(teacherId)) return Forbid();
            return Json(await _exerciseService.GradeAsync(request));
        }

        // ===== لیست حضور و غیاب =====
        public IActionResult AttendanceReport() => View();

        [HttpGet]
        public async Task<IActionResult> GetAttendanceReport([FromQuery] int? classId, [FromQuery] int? sessionId)
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            if (teacherId == null) return Json(new List<AttendanceReportViewModel>());
            return Json(await _attendanceService.GetReportAsync(teacherId.Value, classId, sessionId));
        }

        private async Task<bool> IsMyClass(int classId)
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            return teacherId != null && await _classService.IsTeacherOfClassAsync(classId, teacherId.Value);
        }

        private async Task<bool> IsMySession(int sessionId)
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            if (teacherId == null) return false;
            var sessionTeacherId = await _sessionService.GetClassTeacherIdAsync(sessionId);
            return sessionTeacherId == teacherId.Value;
        }

        private async Task<bool> IsMyTeacherId(int? sessionTeacherId)
        {
            var teacherId = await _currentUser.GetTeacherIdAsync();
            return teacherId != null && sessionTeacherId == teacherId.Value;
        }
    }
}