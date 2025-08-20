namespace YesterdayNews.Utils
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailWithPdfAsync(string email, string subject, string htmlMessage, byte[] pdfBytes, string pdfFileName);
    }
}
