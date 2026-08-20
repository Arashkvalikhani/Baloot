using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationViewModel>> GetMyAsync();
        Task<int> GetUnreadCountAsync();
        Task<AjaxResult> MarkReadAsync(int id);
        Task<AjaxResult> MarkAllReadAsync();
    }
}