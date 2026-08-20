using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService) => _teacherService = teacherService;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetTeachers([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _teacherService.GetPagedAsync(search, pageNumber, pageSize));

        [HttpGet]
        public async Task<IActionResult> GetAll() => Json(await _teacherService.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _teacherService.GetByIdAsync(id);
            return t == null ? NotFound() : Json(t);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] TeacherViewModel model)
            => ModelState.IsValid ? Json(await _teacherService.CreateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] TeacherViewModel model)
            => ModelState.IsValid ? Json(await _teacherService.UpdateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] IdRequest request)
            => Json(await _teacherService.DeleteAsync(request.Id));

        private string GetErrors() => string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
    }
}