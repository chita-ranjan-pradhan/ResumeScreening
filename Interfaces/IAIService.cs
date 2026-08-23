using ResumeScreening.Models;

namespace ResumeScreening.Interfaces
{
    public interface IAIService
    {
        Task<AIResumeAnalysisResult> AnalyzeResumeAsync(
            string jobDescription,
            string resumeText);
    }
}