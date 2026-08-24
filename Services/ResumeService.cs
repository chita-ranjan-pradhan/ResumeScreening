using ResumeScreening.Interfaces;
using ResumeScreening.Models;

namespace ResumeScreening.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IEnumerable<IResumeParser> _parsers;
        private readonly IResumeMatcher _resumeMatcher;

        public ResumeService(
            IEnumerable<IResumeParser> parsers,
            IResumeMatcher resumeMatcher)
        {
            _parsers = parsers;
            _resumeMatcher = resumeMatcher;
        }

        public async Task<List<ResumeAnalysisResult>> Analyze(
            AnalyzeResumeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.JobDescription))
            {
                throw new ArgumentException(
                    "Job description is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ResumeFolderPath))
            {
                throw new ArgumentException(
                    "Resume folder path is required.");
            }

            if (request.ShortlistThreshold < 0 ||
                request.ShortlistThreshold > 100)
            {
                throw new ArgumentException(
                    "Shortlist threshold must be between 0 and 100.");
            }

            if (!Directory.Exists(request.ResumeFolderPath))
            {
                throw new DirectoryNotFoundException(
                    $"Resume folder does not exist: {request.ResumeFolderPath}");
            }

            var files = Directory.GetFiles(
                request.ResumeFolderPath,
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
                         _resumeMatcher.Analyze(
                            request.JobDescription,
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
                        analysis.MatchScore >=
                        request.ShortlistThreshold
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