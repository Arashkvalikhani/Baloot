using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<AttendanceEditViewModel>> GetSessionAttendanceAsync(int sessionId);
        Task<AjaxResult> SaveAsync(int sessionId, List<AttendanceEditViewModel> items);
        Task<List<AttendanceReportViewModel>> GetReportAsync(int teacherId, int? classId, int? sessionId);
        Task<List<AttendanceReportViewModel>> GetByStudentAsync(int studentId);
    }
}