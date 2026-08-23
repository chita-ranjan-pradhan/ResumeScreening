namespace ResumeScreening.Models
{
    public class ResumeTextResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
