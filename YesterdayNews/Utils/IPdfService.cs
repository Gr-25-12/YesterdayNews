namespace YesterdayNews.Utils
{
    public interface IPdfService
    {
        byte[] GenerateReceiptPdf(string userName, string planName, decimal amount, string transactionId);

    }
}
