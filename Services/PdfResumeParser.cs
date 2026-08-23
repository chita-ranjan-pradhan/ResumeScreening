using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using ResumeScreening.Interfaces;

namespace ResumeScreening.Services
{
    public class PdfResumeParser : IResumeParser 
    {
        public bool CanHandle(string extension)
        {
            return extension.Equals(
                ".pdf",
                StringComparison.OrdinalIgnoreCase);
        }

        public Task<string> ExtractTextAsync(string filePath)
        {
            using var reader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(reader);

            var text = new System.Text.StringBuilder();

            for (int page = 1;
                 page <= pdfDocument.GetNumberOfPages();
                 page++)
            {
                var pageText =
                    PdfTextExtractor.GetTextFromPage(
                        pdfDocument.GetPage(page));

                text.AppendLine(pageText);
            }

            return Task.FromResult(text.ToString());
        }
    }
}
