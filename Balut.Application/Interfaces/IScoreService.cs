using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IScoreService
    {
        Task<List<ScoreEditViewModel>> GetSessionScoresAsync(int sessionId);
        Task<AjaxResult> SaveSessionScoresAsync(int sessionId, List<ScoreEditViewModel> items);
        Task<List<ScoreReportViewModel>> GetByStudentAsync(int studentId);
    }
}