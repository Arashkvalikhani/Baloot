using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class ParentController : Controller
    {
        private readonly IParentAdminService _parentAdminService;

        public ParentController(IParentAdminService parentAdminService)
        {
            _parentAdminService = parentAdminService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetParents([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _parentAdminService.GetPagedAsync(search, pageNumber, pageSize));

        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var p = await _parentAdminService.GetDetailAsync(id);
            return p == null ? NotFound() : Json(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] ParentViewModel model)
            => ModelState.IsValid ? Json(await _parentAdminService.CreateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] ParentViewModel model)
            => ModelState.IsValid ? Json(await _parentAdminService.UpdateAsync(model)) : Json(new AjaxResult { Success = false, Message = GetErrors() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus([FromBody] IdRequest request)
            => Json(await _parentAdminService.ToggleStatusAsync(request.Id));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild([FromBody] AddChildRequest request)
            => Json(await _parentAdminService.AddChildAsync(request.ParentId, request.StudentId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveChild([FromBody] RemoveChildRequest request)
            => Json(await _parentAdminService.RemoveChildAsync(request.ParentId, request.StudentId));

        private string GetErrors() => string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
    }
}