using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Secretary")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // صفحه اصلی (اسکلت - داده‌ها با AJAX لود می‌شوند)
        public IActionResult Index() => View();

        // دریافت لیست با جستجو، فیلتر و صفحه‌بندی
        [HttpGet]
        public async Task<IActionResult> GetStudents([FromQuery] StudentFilterViewModel filter)
        {
            var result = await _studentService.GetPagedAsync(filter);
            return Json(result);
        }

        // دریافت همه دانشجویان فعال (برای Dropdownها مثل ثبت‌نام)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentService.GetAllAsync();
            return Json(students);
        }

        // دریافت یک دانشجو برای ویرایش
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
                return NotFound(new AjaxResult { Success = false, Message = "دانشجو یافت نشد." });
            return Json(student);
        }

        // ایجاد دانشجو
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] StudentViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new AjaxResult { Success = false, Message = GetErrors() });

            var result = await _studentService.CreateAsync(model);
            return Json(result);
        }

        // ویرایش دانشجو
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] StudentViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new AjaxResult { Success = false, Message = GetErrors() });

            var result = await _studentService.UpdateAsync(model);
            return Json(result);
        }

        // حذف (غیرفعال‌سازی) دانشجو
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] IdRequest request)
        {
            var result = await _studentService.DeleteAsync(request.Id);
            return Json(result);
        }

        // تغییر وضعیت فعال/غیرفعال
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus([FromBody] IdRequest request)
        {
            var result = await _studentService.ToggleStatusAsync(request.Id);
            return Json(result);
        }

        private string GetErrors()
        {
            return string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
}