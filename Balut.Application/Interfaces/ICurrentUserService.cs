namespace Balut.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        Task<int?> GetTeacherIdAsync();
        Task<int?> GetStudentIdAsync();
        Task<int?> GetParentIdAsync();
    }
}