using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<List<ExerciseViewModel>> GetBySessionAsync(int sessionId);
        Task<AjaxResult> CreateAsync(CreateExerciseRequest model);
        Task<AjaxResult> DeleteAsync(int id);
        Task<int?> GetTeacherIdByExerciseAsync(int exerciseId);
        Task<int?> GetTeacherIdBySubmissionAsync(int submissionId);
        Task<List<SubmissionViewModel>> GetSubmissionsAsync(int exerciseId);
        Task<AjaxResult> SubmitAsync(int exerciseId, int studentId, string? text, Microsoft.AspNetCore.Http.IFormFile? file);
        Task<AjaxResult> GradeAsync(GradeSubmissionRequest model);
        Task<List<StudentExerciseViewModel>> GetByStudentAsync(int studentId);
    }
}