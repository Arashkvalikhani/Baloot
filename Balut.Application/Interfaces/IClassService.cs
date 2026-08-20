using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IClassService
    {
        Task<PagedResult<ClassViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize);
        Task<ClassViewModel?> GetByIdAsync(int id);
        Task<List<ClassViewModel>> GetAllAsync();
        Task<List<ClassViewModel>> GetByTeacherIdAsync(int teacherId);
        Task<bool> IsTeacherOfClassAsync(int classId, int teacherId);
        Task<AjaxResult> CreateAsync(ClassViewModel model);
        Task<AjaxResult> UpdateAsync(ClassViewModel model);
        Task<AjaxResult> DeleteAsync(int id);
    }
}