using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentPanelController : Controller
    {
        private readonly IParentService _parentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IScoreService _scoreService;

        public ParentPanelController(
            IParentService parentService,
            IEnrollmentService enrollmentService,
            IAttendanceService attendanceService,
            IScoreService scoreService)
        {
            _parentService = parentService;
            _enrollmentService = enrollmentService;
            _attendanceService = attendanceService;
            _scoreService = scoreService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetMyChildren()
            => Json(await _parentService.GetMyChildrenAsync());

        public async Task<IActionResult> ChildDetail(int studentId)
        {
            if (!await _parentService.IsParentOfStudentAsync(studentId)) return Forbid();
            ViewData["StudentId"] = studentId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChildEnrollments(int studentId)
        {
            if (!await _parentService.IsParentOfStudentAsync(studentId)) return Forbid();
            return Json(await _enrollmentService.GetByStudentAsync(studentId));
        }

        [HttpGet]
        public async Task<IActionResult> GetChildAttendance(int studentId)
        {
            if (!await _parentService.IsParentOfStudentAsync(studentId)) return Forbid();
            return Json(await _attendanceService.GetByStudentAsync(studentId));
        }

        [HttpGet]
        public async Task<IActionResult> GetChildScores(int studentId)
        {
            if (!await _parentService.IsParentOfStudentAsync(studentId)) return Forbid();
            return Json(await _scoreService.GetByStudentAsync(studentId));
        }
    }
}