using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResult<StudentViewModel>> GetPagedAsync(StudentFilterViewModel filter);
        Task<StudentViewModel?> GetByIdAsync(int id);
        Task<List<StudentViewModel>> GetAllAsync();
        Task<AjaxResult> CreateAsync(StudentViewModel model);
        Task<AjaxResult> UpdateAsync(StudentViewModel model);
        Task<AjaxResult> DeleteAsync(int id);
        Task<AjaxResult> ToggleStatusAsync(int id);
    }
} 