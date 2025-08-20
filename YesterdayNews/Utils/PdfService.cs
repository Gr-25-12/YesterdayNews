using SelectPdf;

namespace YesterdayNews.Utils
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateReceiptPdf(string userName, string planName, decimal amount, string transactionId)
        {
            var htmlContent = EmailTemplate.GetReceiptHtml(userName, planName, amount, transactionId);

            var converter = new HtmlToPdf();

            //oPtions 
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;


            var pdfDocument = converter.ConvertHtmlString(htmlContent);
            return pdfDocument.Save();
        }


    }
}
