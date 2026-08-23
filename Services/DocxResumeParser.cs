using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ResumeScreening.Interfaces;
using System.Text;

namespace ResumeScreening.Services
{
    public class DocxResumeParser : IResumeParser
    {
        public bool CanHandle(string extension)
        {
            return extension.Equals(
                ".docx",
                StringComparison.OrdinalIgnoreCase);
        }

        public Task<string> ExtractTextAsync(string filePath)
        {
            using var document =
                WordprocessingDocument.Open(filePath, false);

            var body =
                document.MainDocumentPart?
                        .Document?
                        .Body;

            if (body == null)
            {
                return Task.FromResult(string.Empty);
            }

            var text = new StringBuilder();

            foreach (var paragraph in
                     body.Elements<Paragraph>())
            {
                text.AppendLine(paragraph.InnerText);
            }

            return Task.FromResult(text.ToString());
        }
    }
}
