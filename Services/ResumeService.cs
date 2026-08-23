using ResumeScreening.Interfaces;
using ResumeScreening.Models;

namespace ResumeScreening.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IEnumerable<IResumeParser> _parsers;
        private readonly IAIService _aiService;

        public ResumeService(
            IEnumerable<IResumeParser> parsers,
            IAIService aiService)
        {
            _parsers = parsers;
            _aiService = aiService;
        }

        public async Task<List<ResumeAnalysisResult>> AnalyzeResumesAsync(
            string jobDescription,
            string folderPath)
        {
            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                throw new ArgumentException(
                    "Job description is required.");
            }

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException(
                    "Resume folder path is required.");
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(
                    $"Resume folder does not exist: {folderPath}");
            }

            var files = Directory.GetFiles(
                folderPath,
                "*.*",
                SearchOption.TopDirectoryOnly);

            var results = new List<ResumeAnalysisResult>();

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);

                var parser = _parsers.FirstOrDefault(
                    x => x.CanHandle(extension));

                if (parser == null)
                {
                    continue;
                }

                var result = new ResumeAnalysisResult
                {
                    FileName = Path.GetFileName(file)
                };

                try
                {
                    // Step 1: Extract resume text
                    var resumeText =
                        await parser.ExtractTextAsync(file);

                    if (string.IsNullOrWhiteSpace(resumeText))
                    {
                        result.Status = "Error";
                        result.ErrorMessage =
                            "Could not extract text from resume.";

                        results.Add(result);
                        continue;
                    }

                    // Step 2: Compare resume with job description
                    var analysis =
                        await _aiService.AnalyzeResumeAsync(
                            jobDescription,
                            resumeText);

                    // Step 3: Build final result
                    result.CandidateName =
                        analysis.CandidateName;

                    result.MatchScore =
                        analysis.MatchScore;

                    result.YearsOfExperience =
                        analysis.YearsOfExperience;

                    result.MatchedSkills =
                        analysis.MatchedSkills;

                    result.MissingSkills =
                        analysis.MissingSkills;

                    result.Reason =
                        analysis.Reason;

                    // Step 4: Decide shortlist status
                    result.Status =
                        analysis.MatchScore >= 70
                            ? "Shortlisted"
                            : "Not Shortlisted";
                }
                catch (Exception ex)
                {
                    result.Status = "Error";
                    result.ErrorMessage = ex.Message;
                }

                results.Add(result);
            }

            return results
                .OrderByDescending(x => x.MatchScore)
                .ToList();
        }
    }
}