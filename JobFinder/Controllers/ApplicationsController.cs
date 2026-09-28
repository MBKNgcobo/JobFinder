using System.Text.Json;

using JobFinder.Models;
using JobFinder.Models.DTOs;
using JobFinder.Models.ViewModels;
using JobFinder.Services;
using JobFinder.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IApplicationAgentService _applicationAgentService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDocumentGenerator _documentGenerator;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public ApplicationsController(
            IApplicationService applicationService,
            IApplicationAgentService applicationAgentService,
            IUserService userService,
            ICurrentUserService currentUserService,
            ApplicationDocumentGenerator documentGenerator)
        {
            _applicationService = applicationService;
            _applicationAgentService = applicationAgentService;
            _userService = userService;
            _currentUserService = currentUserService;
            _documentGenerator = documentGenerator;
        }

        // ============================================================
        // INDEX
        // ============================================================

        // GET: /Applications
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var applications =
                await _applicationService
                    .GetUserApplicationsAsync(userId);

            return View(applications);
        }


        // ============================================================
        // APPLY
        // ============================================================

        // POST: /Applications/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            [FromForm] int jobId)
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            if (jobId <= 0)
            {
                return BadRequest(
                    "A valid job is required.");
            }

            // CreateApplicationAsync already checks whether the
            // user has applied to this job before.
            var application =
                await _applicationService
                    .CreateApplicationAsync(
                        userId,
                        jobId);

            if (application == null)
            {
                return NotFound();
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = application.Id
                });
        }


        // ============================================================
        // DETAILS
        // ============================================================

        // GET: /Applications/Details/1
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var application =
                await _applicationService
                    .GetApplicationByIdAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            // Security boundary
            if (application.UserId != userId)
            {
                return Forbid();
            }

            var model =
                new ApplicationDetailsViewModel
                {
                    Id = application.Id,

                    JobId = application.JobId,

                    Status =
                        string.IsNullOrWhiteSpace(
                            application.Status)
                            ? "Draft"
                            : application.Status,

                    AppliedAt =
                        application.AppliedAt,

                    UpdatedAt =
                        application.UpdatedAt,

                    GeneratedCoverLetter =
                        application.GeneratedCoverLetter,

                    GeneratedCvChanges =
                        application.GeneratedCvChanges,

                    GeneratedMatchedSkills =
                        application.GeneratedMatchedSkills,

                    GeneratedProjects =
                        application.GeneratedProjects,

                    GeneratedAt =
                        application.GeneratedAt,

                    Job =
                        application.Job,

                    MatchedSkills =
                        DeserializeList(
                            application.GeneratedMatchedSkills),

                    CvChanges =
                        DeserializeList(
                            application.GeneratedCvChanges),

                    RecommendedProjects =
                        DeserializeProjects(
                            application.GeneratedProjects),

                    Documents =
                        application.Documents
                            .OrderByDescending(
                                d => d.CreatedAt)
                            .Select(
                                d =>
                                    new ApplicationDocumentViewModel
                                    {
                                        Id = d.Id,
                                        DocumentType =
                                            d.DocumentType,
                                        Version =
                                            d.Version,
                                        CreatedAt =
                                            d.CreatedAt
                                    })
                            .ToList()
                };

            return View(model);
        }


        // ============================================================
        // GENERATE APPLICATION
        // ============================================================

        // POST: /Applications/Generate/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int id)
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Load application
            // --------------------------------------------------------

            var application =
                await _applicationService
                    .GetApplicationByIdAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Security
            // --------------------------------------------------------

            if (application.UserId != userId)
            {
                return Forbid();
            }

            // --------------------------------------------------------
            // Prevent regeneration of completed applications
            // --------------------------------------------------------

            if (string.Equals(
                    application.Status,
                    "Applied",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "This application has already been marked as applied.");
            }

            // --------------------------------------------------------
            // Load candidate
            // --------------------------------------------------------

            var user =
                await _userService
                    .GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Validate job
            // --------------------------------------------------------

            var job = application.Job;

            if (job == null)
            {
                return BadRequest(
                    "The application does not contain a valid job.");
            }

            // ========================================================
            // BUILD CANDIDATE PROFILE
            // ========================================================

            var userProfile =
                new UserProfileDto
                {
                    FirstName =
                        user.FirstName,

                    LastName =
                        user.LastName,

                    Summary =
                        user.Summary,

                    YearsOfExperience =
                        user.YearsOfExperience,

                    Skills =
                        user.UserSkills
                            .Where(
                                us => us.Skill != null)
                            .Select(
                                us => us.Skill!.Name)
                            .ToList(),

                    Projects =
                        user.Projects
                            .Select(
                                project =>
                                    new ProjectDto
                                    {
                                        Name =
                                            project.Name,

                                        Skills =
                                            project.ProjectSkills
                                                .Where(
                                                    ps =>
                                                        ps.Skill != null)
                                                .Select(
                                                    ps =>
                                                        ps.Skill!.Name)
                                                .ToList()
                                    })
                            .ToList()
                };

            // ========================================================
            // BUILD JOB PROFILE
            // ========================================================

            var jobRequest =
                new JobApplicationDto
                {
                    Title =
                        job.Title,

                    Company =
                        job.Company?.Name
                        ?? "Unknown Company",

                    Description =
                        job.Description
                        ?? string.Empty,

                    RequiredSkills =
                        job.JobSkills
                            .Where(
                                js =>
                                    js.IsRequired &&
                                    js.Skill != null)
                            .Select(
                                js =>
                                    js.Skill!.Name)
                            .ToList(),

                    PreferredSkills =
                        job.JobSkills
                            .Where(
                                js =>
                                    !js.IsRequired &&
                                    js.Skill != null)
                            .Select(
                                js =>
                                    js.Skill!.Name)
                            .ToList()
                };

            // ========================================================
            // BUILD APPLICATION AGENT REQUEST
            // ========================================================

            var request =
                new ApplicationAgentRequestDto
                {
                    UserProfile =
                        userProfile,

                    Job =
                        jobRequest
                };

            try
            {
                // ====================================================
                // RUN AI APPLICATION PIPELINE
                // ====================================================

                var result =
                    await _applicationAgentService
                        .GenerateApplicationAsync(
                            request);

                if (result == null)
                {
                    return BadRequest(
                        "Application Agent returned an empty response.");
                }

                // ====================================================
                // SAVE GENERATED CONTENT
                // ====================================================

                application.GeneratedCoverLetter =
                    result.CoverLetter;

                application.GeneratedCvChanges =
                    JsonSerializer.Serialize(
                        result.CvChanges,
                        JsonOptions);

                application.GeneratedMatchedSkills =
                    JsonSerializer.Serialize(
                        result.MatchedSkills,
                        JsonOptions);

                application.GeneratedProjects =
                    JsonSerializer.Serialize(
                        result.RecommendedProjects,
                        JsonOptions);

                application.CoverLetter =
                    result.CoverLetter;

                application.GeneratedAt =
                    DateTime.UtcNow;

                application.UpdatedAt =
                    DateTime.UtcNow;

                // ====================================================
                // GENERATE CV DOCUMENT
                // ====================================================

                var cvBytes =
                    _documentGenerator.GenerateCv(
                        user,
                        application,
                        result);

                // ====================================================
                // GENERATE COVER LETTER DOCUMENT
                // ====================================================

                var coverLetterBytes =
                    _documentGenerator.GenerateCoverLetter(
                        user,
                        application,
                        result);

                // ====================================================
                // DETERMINE NEXT VERSIONS
                // ====================================================

                var existingCvVersion =
                    application.Documents
                        .Where(
                            d =>
                                d.DocumentType == "CV")
                        .Select(
                            d =>
                                d.Version)
                        .DefaultIfEmpty(0)
                        .Max();

                var existingCoverLetterVersion =
                    application.Documents
                        .Where(
                            d =>
                                d.DocumentType ==
                                "CoverLetter")
                        .Select(
                            d =>
                                d.Version)
                        .DefaultIfEmpty(0)
                        .Max();

                // ====================================================
                // SAVE CV DOCUMENT
                // ====================================================

                application.Documents.Add(
                    new ApplicationDocument
                    {
                        DocumentType = "CV",

                        Version =
                            existingCvVersion + 1,

                        Content =
                            Convert.ToBase64String(
                                cvBytes),

                        CreatedAt =
                            DateTime.UtcNow
                    });

                // ====================================================
                // SAVE COVER LETTER DOCUMENT
                // ====================================================

                application.Documents.Add(
                    new ApplicationDocument
                    {
                        DocumentType =
                            "CoverLetter",

                        Version =
                            existingCoverLetterVersion + 1,

                        Content =
                            Convert.ToBase64String(
                                coverLetterBytes),

                        CreatedAt =
                            DateTime.UtcNow
                    });

                // ====================================================
                // PERSIST EVERYTHING
                // ====================================================

                var saved =
                    await _applicationService
                        .UpdateApplicationAsync(
                            application);

                if (!saved)
                {
                    return BadRequest(
                        "Failed to save generated application.");
                }

                // ====================================================
                // RETURN TO APPLICATION DETAILS
                // ====================================================

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = application.Id
                    });
            }
            catch (HttpRequestException exception)
            {
                return BadRequest(
                    $"Application service communication failed: {exception.Message}");
            }
            catch (TaskCanceledException)
            {
                return BadRequest(
                    "Application generation timed out. Please try again.");
            }
            catch (Exception exception)
            {
                return BadRequest(
                    $"Application generation failed: {exception.Message}");
            }
        }


        // ============================================================
        // APPROVE / MARK AS APPLIED
        // ============================================================

        // POST: /Applications/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var application =
                await _applicationService
                    .GetApplicationByIdAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            // Security
            if (application.UserId != userId)
            {
                return Forbid();
            }

            // Already applied
            if (string.Equals(
                    application.Status,
                    "Applied",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = application.Id
                    });
            }

            // Do not allow approval without generated content
            if (string.IsNullOrWhiteSpace(
                    application.GeneratedCoverLetter))
            {
                return BadRequest(
                    "Generate and review the application before marking it as applied.");
            }

            // Final state
            application.Status = "Applied";

            application.AppliedAt =
                DateTime.UtcNow;

            application.UpdatedAt =
                DateTime.UtcNow;

            var saved =
                await _applicationService
                    .UpdateApplicationAsync(
                        application);

            if (!saved)
            {
                return BadRequest(
                    "Failed to update application status.");
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = application.Id
                });
        }


        // ============================================================
        // DOWNLOAD DOCUMENT
        // ============================================================

        // GET:
        // /Applications/DownloadDocument?applicationId=1&documentId=1
        [HttpGet]
        public async Task<IActionResult> DownloadDocument(
            int applicationId,
            int documentId)
        {
            var application =
                await _applicationService
                    .GetApplicationByIdAsync(
                        applicationId);

            if (application == null)
            {
                return NotFound();
            }

            // Security
            if (application.UserId !=
                _currentUserService.UserId)
            {
                return Forbid();
            }

            var document =
                application.Documents
                    .FirstOrDefault(
                        document =>
                            document.Id ==
                            documentId);

            if (document == null)
            {
                return NotFound();
            }

            byte[] bytes;

            try
            {
                bytes =
                    Convert.FromBase64String(
                        document.Content);
            }
            catch
            {
                return BadRequest(
                    "Stored document content is invalid.");
            }

            var jobTitle =
                application.Job?.Title
                ?? "Application";

            var safeJobTitle =
                string.Join(
                    "_",
                    jobTitle.Split(
                        Path.GetInvalidFileNameChars(),
                        StringSplitOptions.RemoveEmptyEntries));

            var fileName =
                $"{safeJobTitle}_" +
                $"{document.DocumentType}_" +
                $"v{document.Version}.docx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }


        // ============================================================
        // JSON HELPERS
        // ============================================================

        private static List<string> DeserializeList(
            string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(
                    json,
                    JsonOptions)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }


        private static List<RecommendedProjectDto>
            DeserializeProjects(
                string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<RecommendedProjectDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<
                    List<RecommendedProjectDto>
                >(
                    json,
                    JsonOptions)
                    ?? new List<RecommendedProjectDto>();
            }
            catch
            {
                return new List<RecommendedProjectDto>();
            }
        }
    }
}