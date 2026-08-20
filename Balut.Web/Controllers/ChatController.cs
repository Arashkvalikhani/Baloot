using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "Teacher,Parent")]
    public class ChatController : Controller
    {
        private readonly IMessageService _messageService;

        public ChatController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetPartners()
            => Json(await _messageService.GetPartnersAsync());

        [HttpGet]
        public async Task<IActionResult> GetThread([FromQuery] string otherUserId, [FromQuery] int studentId)
            => Json(await _messageService.GetThreadAsync(otherUserId, studentId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
            => Json(await _messageService.SendAsync(request));

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
            => Json(new { count = await _messageService.GetUnreadCountAsync() });
    }
}