using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IEnrollmentService
    {
        Task<PagedResult<EnrollmentViewModel>> GetPagedAsync(string? search, int pageNumber, int pageSize);
        Task<List<EnrollmentViewModel>> GetByClassAsync(int classId);
        Task<List<EnrollmentViewModel>> GetByStudentAsync(int studentId);
        Task<AjaxResult> CreateAsync(int studentId, int classId);
        Task<AjaxResult> UpdateAsync(EnrollmentUpdateViewModel model);
        Task<AjaxResult> DropAsync(int id);
        Task<AjaxResult> DeleteAsync(int id);
    }
}