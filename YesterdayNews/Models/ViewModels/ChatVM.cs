using System.ComponentModel.DataAnnotations;

namespace YesterdayNews.Models.ViewModels
{
    public class ChatRequestViewModel
    {
        [Required]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;

        public List<ChatMessageViewModel>? ConversationHistory { get; set; }
    }

    public class ChatMessageViewModel
    {
        [Required]
        public string Type { get; set; } = string.Empty; // "user" or "bot"

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }

    public class ChatResponseViewModel
    {
        public bool Success { get; set; }
        public string Response { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
