namespace ResumeScreening.Models
{
    public class AIResumeAnalysisResult
    {
        public string CandidateName { get; set; } = string.Empty;

        public int MatchScore { get; set; }

        public double YearsOfExperience { get; set; }

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public string Reason { get; set; } = string.Empty;
    }
}