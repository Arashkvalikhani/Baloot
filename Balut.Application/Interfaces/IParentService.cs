using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IParentService
    {
        Task<List<ParentChildViewModel>> GetMyChildrenAsync();
        Task<bool> IsParentOfStudentAsync(int studentId);
    }
}