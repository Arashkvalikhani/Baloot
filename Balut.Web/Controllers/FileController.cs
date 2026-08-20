using Balut.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _env;

        public FileController(IFileService fileService, IWebHostEnvironment env)
        {
            _fileService = fileService;
            _env = env;
        }

        [HttpGet("/File/Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            // کارکنان دسترسی کامل دارند
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("Secretary"))
            {
                var staffFile = await _fileService.GetForDownloadAsync(id);
                if (staffFile == null) return NotFound();
                return ServeFile(staffFile.FilePath, staffFile.ContentType, staffFile.FileName);
            }

            var file = await _fileService.GetForDownloadAsync(id);
            if (file == null) return Forbid();

            return ServeFile(file.FilePath, file.ContentType, file.FileName);
        }

        private IActionResult ServeFile(string relativePath, string contentType, string downloadName)
        {
            var physical = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (!System.IO.File.Exists(physical)) return NotFound();
            return PhysicalFile(physical, contentType, downloadName);
        }
    }
}