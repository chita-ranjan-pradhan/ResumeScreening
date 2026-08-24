using ResumeScreening.Models;

namespace ResumeScreening.Interfaces
{
    public interface IResumeService
    {
        Task<List<ResumeAnalysisResult>> Analyze(
            AnalyzeResumeRequest request);
    }
}