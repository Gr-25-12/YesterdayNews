using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{

    public interface INewsChatService
    {
        Task<string> GetNewsResponseAsync(string userMessage, List<ChatMessageViewModel> conversationHistory);
    }
    
}
