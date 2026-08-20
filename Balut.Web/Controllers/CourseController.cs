using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService) => _courseService = courseService;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _courseService.GetPagedAsync(search, pageNumber, pageSize));

        [HttpGet]
        public async Task<IActionResult> GetAll() => Json(await _courseService.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _courseService.GetByIdAsync(id);
            return c == null ? NotFound() : Json(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CourseViewModel model)
            => ModelState.IsValid ? Json(await _courseService.CreateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] CourseViewModel model)
            => ModelState.IsValid ? Json(await _courseService.UpdateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] IdRequest request)
            => Json(await _courseService.DeleteAsync(request.Id));

        private string GetErrors() => string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
    }
}