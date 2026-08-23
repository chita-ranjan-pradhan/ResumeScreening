namespace ResumeScreening.Models
{
    public class ResumeAnalysisResult
    {
        public string FileName { get; set; } = string.Empty;

        public string CandidateName { get; set; } = string.Empty;

        public int MatchScore { get; set; }

        public string Status { get; set; } = string.Empty;

        public double YearsOfExperience { get; set; }

        public List<string> MatchedSkills { get; set; } = new();

        public List<string> MissingSkills { get; set; } = new();

        public string Reason { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}