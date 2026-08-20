using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardAsync();
    }
}