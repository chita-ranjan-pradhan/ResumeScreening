using ResumeScreening.Interfaces;
using ResumeScreening.Models;

namespace ResumeScreening.Services
{
    public class OpenAIService : IAIService
    {
        public Task<AIResumeAnalysisResult> AnalyzeResumeAsync(
            string jobDescription,
            string resumeText)
        {
            // AI logic will be added next.
            // For now, return a dummy result.

            var result = new AIResumeAnalysisResult
            {
                CandidateName = "Test Candidate",
                MatchScore = 0,
                YearsOfExperience = 0,
                MatchedSkills = new List<string>(),
                MissingSkills = new List<string>(),
                Reason = "AI matching has not been implemented yet."
            };

            return Task.FromResult(result);
        }
    }
}