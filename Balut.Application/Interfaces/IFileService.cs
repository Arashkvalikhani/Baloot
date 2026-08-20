using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Balut.Application.Interfaces
{
    public interface IFileService
    {
        Task<AjaxResult> SaveAsync(IFormFile file, int entityId, string entityType);
        Task<FileViewModel?> GetForDownloadAsync(int fileId);
    }
}