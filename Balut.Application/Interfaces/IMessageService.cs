using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IMessageService
    {
        Task<List<PartnerViewModel>> GetPartnersAsync();
        Task<List<MessageViewModel>> GetThreadAsync(string otherUserId, int studentId);
        Task<AjaxResult> SendAsync(SendMessageRequest request);
        Task<int> GetUnreadCountAsync();
    }
}