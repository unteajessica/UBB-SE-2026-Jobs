namespace Tests_and_Interviews.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews.Web.Clients;
    using Tests_and_Interviews_API.Dtos;

    /// <summary>
    /// Handles all applicant-related pages. Delegates all data operations to <see cref="ApplicantsApiClient"/>.
    /// </summary>
    [Authorize]
    public class ApplicantsController : Controller
    {
        private readonly ApplicantsApiClient applicantsApiClient;
        private readonly JobsApiClient jobsApiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicantsController"/> class.
        /// </summary>
        /// <param name="applicantsApiClient">The API proxy client for applicants.</param>
        /// <param name="jobsApiClient">The API proxy client for jobs.</param>
        public ApplicantsController(ApplicantsApiClient applicantsApiClient, JobsApiClient jobsApiClient)
        {
            this.applicantsApiClient = applicantsApiClient;
            this.jobsApiClient = jobsApiClient;
        }

        /// <summary>
        /// Displays the apply form for a specific job. Candidate only.
        /// </summary>
        /// <param name="jobId">The job ID to apply for.</param>
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Apply(int jobId)
        {
            try
            {
                this.AttachJwt();

                // Get job details to display
                JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(jobId);
                if (job == null)
                {
                    return this.NotFound();
                }

                this.ViewBag.Job = job;

                // Check if user has already applied
                int userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return this.Unauthorized();
                }

                bool hasApplied = await this.applicantsApiClient.HasUserAppliedAsync(jobId, userId);
                this.ViewBag.AlreadyApplied = hasApplied;

                return this.View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Apply action: {ex.Message}");
                return this.StatusCode(500, "An error occurred while loading the application form.");
            }
        }

        /// <summary>
        /// Handles the apply form submission. Candidate only.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> SubmitApplication(int jobId)
        {
            try
            {
                this.AttachJwt();

                int userId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return this.Unauthorized();
                }

                // Check if user has already applied
                bool hasApplied = await this.applicantsApiClient.HasUserAppliedAsync(jobId, userId);
                if (hasApplied)
                {
                    this.ModelState.AddModelError(string.Empty, "You have already applied for this job.");
                    JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(jobId);
                    this.ViewBag.Job = job;
                    this.ViewBag.AlreadyApplied = true;
                    return this.View("Apply");
                }

                // Create the applicant
                ApplicantDto dto = new ApplicantDto
                {
                    JobId = jobId,
                    UserId = userId,
                    AppliedAt = System.DateTime.UtcNow,
                    ApplicationStatus = "Pending"
                };

                ApplicantDto? result = await this.applicantsApiClient.CreateApplicantAsync(dto);
                if (result == null)
                {
                    this.ModelState.AddModelError(string.Empty, "Failed to submit application. Please try again.");
                    JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(jobId);
                    this.ViewBag.Job = job;
                    this.ViewBag.JobId = jobId;
                    return this.View("Apply");
                }

                this.TempData["SuccessMessage"] = "Application submitted successfully!";
                return this.RedirectToAction("Details", "Jobs", new { id = jobId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SubmitApplication action: {ex.Message}");
                this.ModelState.AddModelError(string.Empty, "An error occurred while submitting your application.");
                return this.View("Apply");
            }
        }

        /// <summary>
        /// Displays all applicants for a specific job. Recruiter only.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> ApplicantsByJob(int jobId)
        {
            this.AttachJwt();

            // Get job details
            JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(jobId);
            if (job == null)
            {
                return this.NotFound();
            }

            // Get applicants for the job
            List<ApplicantDto> applicants = await this.applicantsApiClient.GetApplicantsByJobAsync(jobId);

            this.ViewBag.Job = job;
            return this.View(applicants);
        }

        /// <summary>
        /// Helper method to attach JWT token from claims to the HTTP client.
        /// </summary>
        private void AttachJwt()
        {
            string? jwt = this.User.FindFirstValue("jwt");
            if (!string.IsNullOrEmpty(jwt))
            {
                this.applicantsApiClient.SetAuthToken(jwt);
                this.jobsApiClient.SetAuthToken(jwt);
            }
        }
    }
}
