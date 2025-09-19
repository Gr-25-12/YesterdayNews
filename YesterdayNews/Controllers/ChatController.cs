using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class ChatController : Controller
    {
        private readonly INewsChatService _newsChatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(INewsChatService newsChatService, ILogger<ChatController> logger)
        {
            _newsChatService = newsChatService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskNews([FromBody] ChatRequestViewModel request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Json(new { success = false, error = "Message cannot be empty." });
                }

                if (request.Message.Length > 500)
                {
                    return Json(new { success = false, error = "Message is too long. Please keep it under 500 characters." });
                }

                var response = await _newsChatService.GetNewsResponseAsync(
                    request.Message,
                    request.ConversationHistory ?? new List<ChatMessageViewModel>()
                );

                return Json(new { success = true, response = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request: {Message}", request.Message);
                return Json(new { success = false, error = "Sorry, I encountered an error processing your request. Please try again." });
            }
        }
    }
}
