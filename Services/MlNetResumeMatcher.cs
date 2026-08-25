using Microsoft.ML;
using ResumeScreening.Interfaces;
using ResumeScreening.Models;
using System.Text.RegularExpressions;

namespace ResumeScreening.Services
{
    public class MlNetResumeMatcher : IResumeMatcher
    {
        private readonly MLContext _mlContext;

        private static readonly Dictionary<string, string[]> SkillGroups =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["C#"] =
                [
                    "c#",
                    "csharp",
                    "c sharp"
                ],

                [".NET"] =
                [
                    ".net",
                    "dotnet",
                    "microsoft .net"
                ],

                ["ASP.NET"] =
                [
                    "asp.net",
                    "asp net",
                    "aspnet"
                ],

                ["ASP.NET Core"] =
                [
                    "asp.net core",
                    "asp net core",
                    "aspnet core"
                ],

                ["Web API"] =
                [
                    "web api",
                    "webapi",
                    "rest api",
                    "restful api",
                    "restful services"
                ],

                ["Entity Framework"] =
                [
                    "entity framework",
                    "entity framework core",
                    "ef core"
                ],

                ["LINQ"] =
                [
                    "linq"
                ],

                ["SQL Server"] =
                [
                    "sql server",
                    "mssql",
                    "ms sql"
                ],

                ["MySQL"] =
                [
                    "mysql"
                ],

                ["PostgreSQL"] =
                [
                    "postgresql",
                    "postgres"
                ],

                ["Angular"] =
                [
                    "angular"
                ],

                ["TypeScript"] =
                [
                    "typescript"
                ],

                ["React"] =
                [
                    "react",
                    "reactjs",
                    "react.js"
                ],

                ["Vue"] =
                [
                    "vue",
                    "vue.js",
                    "vuejs"
                ],

                ["Java"] =
                [
                    "java"
                ],

                ["Spring Boot"] =
                [
                    "spring boot",
                    "springboot"
                ],

                ["Python"] =
                [
                    "python"
                ],

                ["Django"] =
                [
                    "django"
                ],

                ["Node.js"] =
                [
                    "node.js",
                    "nodejs",
                    "node"
                ],

                ["JavaScript"] =
                [
                    "javascript",
                    "js"
                ],

                ["Redis"] =
                [
                    "redis"
                ],

                ["Docker"] =
                [
                    "docker"
                ],

                ["Kubernetes"] =
                [
                    "kubernetes",
                    "k8s"
                ],

                ["Azure"] =
                [
                    "azure",
                    "microsoft azure"
                ],

                ["AWS"] =
                [
                    "aws",
                    "amazon web services"
                ],

                ["Git"] =
                [
                    "git"
                ],

                ["GitLab"] =
                [
                    "gitlab"
                ],

                ["GitHub"] =
                [
                    "github"
                ]
            };

        public MlNetResumeMatcher()
        {
            _mlContext = new MLContext(seed: 1);
        }

        public ResumeAnalysisResult Analyze(
            string jobDescription,
            string resumeText)
        {
            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                throw new ArgumentException(
                    "Job description is required.");
            }

            if (string.IsNullOrWhiteSpace(resumeText))
            {
                throw new ArgumentException(
                    "Resume text is required.");
            }

            var jobSkills = ExtractSkills(jobDescription);

            var resumeSkills = ExtractSkills(resumeText);

            var matchedSkills = jobSkills
                .Where(skill => resumeSkills.Contains(skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var missingSkills = jobSkills
                .Where(skill => !resumeSkills.Contains(skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var skillScore = CalculateSkillScore(
                jobSkills,
                matchedSkills);

            var semanticScore = CalculateSemanticScore(
                jobDescription,
                resumeText);

            var finalScore = CalculateFinalScore(
                skillScore,
                semanticScore);

            var yearsOfExperience =
                ExtractYearsOfExperience(resumeText);

            return new ResumeAnalysisResult
            {
                CandidateName =
                    ExtractCandidateName(resumeText),

                MatchScore = finalScore,

                YearsOfExperience =
                    yearsOfExperience,

                MatchedSkills = matchedSkills,

                MissingSkills = missingSkills,

                Reason = BuildReason(
                    finalScore,
                    matchedSkills,
                    missingSkills)
            };
        }

        private double CalculateSkillScore(
            List<string> jobSkills,
            List<string> matchedSkills)
        {
            if (jobSkills.Count == 0)
            {
                return 50;
            }

            return
                (double)matchedSkills.Count /
                jobSkills.Count *
                100;
        }

        private int CalculateSemanticScore(
            string jobDescription,
            string resumeText)
        {
            var data = new[]
            {
                new TextData
                {
                    Text = jobDescription
                },

                new TextData
                {
                    Text = resumeText
                }
            };

            var dataView =
                _mlContext.Data.LoadFromEnumerable(data);

            var pipeline =
                _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features",
                    inputColumnName: nameof(TextData.Text));

            var transformer =
                pipeline.Fit(dataView);

            var transformedData =
                transformer.Transform(dataView);

            var vectors =
                _mlContext.Data
                    .CreateEnumerable<FeatureData>(
                        transformedData,
                        reuseRowObject: false)
                    .ToList();

            if (vectors.Count != 2)
            {
                return 0;
            }

            var similarity =
                CosineSimilarity(
                    vectors[0].Features,
                    vectors[1].Features);

            return Math.Clamp(
                (int)Math.Round(similarity * 100),
                0,
                100);
        }

        private static double CosineSimilarity(
            float[] vectorA,
            float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
            {
                return 0;
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (var i = 0; i < vectorA.Length; i++)
            {
                dotProduct +=
                    vectorA[i] * vectorB[i];

                magnitudeA +=
                    vectorA[i] * vectorA[i];

                magnitudeB +=
                    vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 ||
                magnitudeB == 0)
            {
                return 0;
            }

            return dotProduct /
                   (Math.Sqrt(magnitudeA) *
                    Math.Sqrt(magnitudeB));
        }

        private static int CalculateFinalScore(
            double skillScore,
            int semanticScore)
        {
            const double skillWeight = 0.70;
            const double semanticWeight = 0.30;

            var finalScore =
                (skillScore * skillWeight) +
                (semanticScore * semanticWeight);

            return Math.Clamp(
                (int)Math.Round(finalScore),
                0,
                100);
        }

        private static List<string> ExtractSkills(
            string text)
        {
            var skills = new List<string>();

            foreach (var skillGroup in SkillGroups)
            {
                foreach (var synonym in skillGroup.Value)
                {
                    if (ContainsTerm(text, synonym))
                    {
                        skills.Add(skillGroup.Key);
                        break;
                    }
                }
            }

            return skills;
        }

        private static bool ContainsTerm(
            string text,
            string term)
        {
            return Regex.IsMatch(
                text,
                $@"(?<![\w#+.]){Regex.Escape(term)}(?![\w#+.])",
                RegexOptions.IgnoreCase);
        }

        private static string ExtractCandidateName(
            string resumeText)
        {
            var lines = resumeText
                .Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (lines.Count == 0)
            {
                return string.Empty;
            }

            // For many resumes the candidate's name is near the top.
            return lines[0];
        }

        private static double ExtractYearsOfExperience(
            string resumeText)
        {
            var patterns = new[]
            {
                @"(\d+(?:\.\d+)?)\s*\+?\s*years?\s*(?:of)?\s*experience",
                @"(\d+(?:\.\d+)?)\s*\+?\s*years?\s*in"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(
                    resumeText,
                    pattern,
                    RegexOptions.IgnoreCase);

                if (match.Success &&
                    double.TryParse(
                        match.Groups[1].Value,
                        out var years))
                {
                    return years;
                }
            }

            return 0;
        }

        private static string BuildReason(
            int score,
            List<string> matchedSkills,
            List<string> missingSkills)
        {
            if (score >= 80)
            {
                return
                    "Strong match based on required skills and overall resume similarity.";
            }

            if (score >= 70)
            {
                return
                    "Good match with most relevant requirements present.";
            }

            if (missingSkills.Count > 0)
            {
                return
                    $"Some job requirements are missing: " +
                    $"{string.Join(", ", missingSkills)}.";
            }

            if (matchedSkills.Count > 0)
            {
                return
                    "Some relevant skills were found, but the overall match is limited.";
            }

            return "Limited match with the job description.";
        }

        private class TextData
        {
            public string Text { get; set; } = string.Empty;
        }

        private class FeatureData
        {
            public float[] Features { get; set; } = [];
        }
    }
}