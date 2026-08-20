using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize);
        Task<CourseViewModel?> GetByIdAsync(int id);
        Task<List<CourseViewModel>> GetAllAsync();
        Task<AjaxResult> CreateAsync(CourseViewModel model);
        Task<AjaxResult> UpdateAsync(CourseViewModel model);
        Task<AjaxResult> DeleteAsync(int id);
    }
}