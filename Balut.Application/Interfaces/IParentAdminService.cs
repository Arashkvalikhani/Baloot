using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IParentAdminService
    {
        Task<PagedResult<ParentViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize);
        Task<ParentDetailViewModel?> GetDetailAsync(int id);
        Task<AjaxResult> CreateAsync(ParentViewModel model);
        Task<AjaxResult> UpdateAsync(ParentViewModel model);
        Task<AjaxResult> ToggleStatusAsync(int id);
        Task<AjaxResult> AddChildAsync(int parentId, int studentId);
        Task<AjaxResult> RemoveChildAsync(int parentId, int studentId);
    }
}