namespace ResumeScreening.Interfaces
{
    public interface IResumeParser
    {
        bool CanHandle(string extension);
        Task<string> ExtractTextAsync(string filePath);
    }
}
