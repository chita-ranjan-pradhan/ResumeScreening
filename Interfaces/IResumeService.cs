using ResumeScreening.Models;

namespace ResumeScreening.Interfaces
{
    public interface IResumeService
    {
        Task<List<ResumeAnalysisResult>> AnalyzeResumesAsync(
            string jobDescription,
            string folderPath);
    }
}
