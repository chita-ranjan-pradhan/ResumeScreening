using Microsoft.AspNetCore.Mvc;
using ResumeScreening.Interfaces;
using ResumeScreening.Models;

namespace ResumeScreening.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;

        public ResumeController(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(
            [FromForm] AnalyzeResumeRequest request)
        {
            try
            {
                var results =
                    await _resumeService.Analyze(request);

                return Ok(new
                {
                    totalFiles = results.Count,

                    shortlistedCount =
                        results.Count(
                            x => x.Status == "Shortlisted"),

                    results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred.",
                    error = ex.Message
                });
            }
        }
    }
}