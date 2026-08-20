using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetEnrollments([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _enrollmentService.GetPagedAsync(search, pageNumber, pageSize));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
            => Json(await _enrollmentService.CreateAsync(request.StudentId, request.ClassId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] EnrollmentUpdateViewModel model)
            => Json(await _enrollmentService.UpdateAsync(model));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Drop([FromBody] IdRequest request)
            => Json(await _enrollmentService.DropAsync(request.Id));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] IdRequest request)
            => Json(await _enrollmentService.DeleteAsync(request.Id));
    }
}