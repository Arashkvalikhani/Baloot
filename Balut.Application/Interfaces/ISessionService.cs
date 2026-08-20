using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface ISessionService
    {
        Task<List<SessionViewModel>> GetByClassAsync(int classId);
        Task<SessionViewModel?> GetByIdAsync(int id);
        Task<int?> GetClassTeacherIdAsync(int sessionId);
        Task<AjaxResult> CreateAsync(SessionViewModel model);
        Task<AjaxResult> UpdateAsync(SessionViewModel model);
        Task<AjaxResult> DeleteAsync(int id);
    }
}