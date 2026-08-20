using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        public ClassController(IClassService classService) => _classService = classService;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetClasses([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _classService.GetPagedAsync(search, pageNumber, pageSize));

        [HttpGet]
        public async Task<IActionResult> GetAll() => Json(await _classService.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _classService.GetByIdAsync(id);
            return c == null ? NotFound() : Json(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] ClassViewModel model)
            => ModelState.IsValid ? Json(await _classService.CreateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] ClassViewModel model)
            => ModelState.IsValid ? Json(await _classService.UpdateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] IdRequest request)
            => Json(await _classService.DeleteAsync(request.Id));

        private string GetErrors() => string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
    }
}