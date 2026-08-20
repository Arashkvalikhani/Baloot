using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<PagedResult<TeacherViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize);
        Task<TeacherViewModel?> GetByIdAsync(int id);
        Task<List<TeacherViewModel>> GetAllAsync();
        Task<AjaxResult> CreateAsync(TeacherViewModel model);
        Task<AjaxResult> UpdateAsync(TeacherViewModel model);
        Task<AjaxResult> DeleteAsync(int id);
    }
}