using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPanelController : Controller
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IScoreService _scoreService;
        private readonly INotificationService _notificationService;
        private readonly IExerciseService _exerciseService;

        public StudentPanelController(
            ICurrentUserService currentUser,
            IEnrollmentService enrollmentService,
            IAttendanceService attendanceService,
            IScoreService scoreService,
            INotificationService notificationService,
            IExerciseService exerciseService)
        {
            _currentUser = currentUser;
            _enrollmentService = enrollmentService;
            _attendanceService = attendanceService;
            _scoreService = scoreService;
            _notificationService = notificationService;
            _exerciseService = exerciseService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var sid = await _currentUser.GetStudentIdAsync();
            if (sid == null) return Json(new List<EnrollmentViewModel>());
            return Json(await _enrollmentService.GetByStudentAsync(sid.Value));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAttendance()
        {
            var sid = await _currentUser.GetStudentIdAsync();
            if (sid == null) return Json(new List<AttendanceReportViewModel>());
            return Json(await _attendanceService.GetByStudentAsync(sid.Value));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyScores()
        {
            var sid = await _currentUser.GetStudentIdAsync();
            if (sid == null) return Json(new List<ScoreReportViewModel>());
            return Json(await _scoreService.GetByStudentAsync(sid.Value));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyExercises()
        {
            var sid = await _currentUser.GetStudentIdAsync();
            if (sid == null) return Json(new List<StudentExerciseViewModel>());
            return Json(await _exerciseService.GetByStudentAsync(sid.Value));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExercise([FromForm] int exerciseId, [FromForm] string? text, IFormFile? file)
        {
            var sid = await _currentUser.GetStudentIdAsync();
            if (sid == null) return Forbid();
            return Json(await _exerciseService.SubmitAsync(exerciseId, sid.Value, text, file));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
            => Json(await _notificationService.GetMyAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead([FromBody] IdRequest request)
            => Json(await _notificationService.MarkReadAsync(request.Id));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
            => Json(await _notificationService.MarkAllReadAsync());
    }
}