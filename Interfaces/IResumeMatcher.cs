using ResumeScreening.Models;

namespace ResumeScreening.Interfaces
{
    public interface IResumeMatcher
    {
        ResumeAnalysisResult Analyze(
            string jobDescription,
            string resumeText);
    }
}