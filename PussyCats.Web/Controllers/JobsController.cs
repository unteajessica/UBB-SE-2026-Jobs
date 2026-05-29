namespace PussyCats.Web.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PussyCats.Web.Clients;
    using PussyCats.Web.Dtos;

    /// <summary>
    /// Handles all job-related pages. Delegates all data operations to <see cref="JobsApiClient"/>.
    /// </summary>
    [Authorize]
    public class JobsController : Controller
    {
        private readonly JobsApiClient jobsApiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobsController"/> class.
        /// </summary>
        /// <param name="jobsApiClient">The API proxy client for jobs.</param>
        public JobsController(JobsApiClient jobsApiClient)
        {
            this.jobsApiClient = jobsApiClient;
        }

        /// <summary>
        /// Displays the list of all job postings.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            this.AttachJwt();
            List<JobPostingDto> jobs = await this.jobsApiClient.GetAllJobsAsync();
            return this.View(jobs);
        }

        /// <summary>
        /// Displays the details of a single job posting.
        /// </summary>
        /// <param name="id">The job posting ID.</param>
        public async Task<IActionResult> Details(int id)
        {
            this.AttachJwt();
            JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(id);
            if (job == null)
            {
                return this.NotFound();
            }

            return this.View(job);
        }

        /// <summary>
        /// Displays the create job form. Recruiter only.
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Create()
        {
            this.AttachJwt();
            List<SkillDto> skills = await this.jobsApiClient.GetAllSkillsAsync();
            this.ViewBag.Skills = skills;
            return this.View(new JobPostingDto());
        }

        /// <summary>
        /// Handles create job form submission. Recruiter only.
        /// </summary>
        /// <param name="dto">The job posting form data.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Create(JobPostingDto dto)
        {
            this.ModelState.Remove("JobSkills");

            if (!this.ModelState.IsValid)
            {
                this.AttachJwt();
                this.ViewBag.Skills = await this.jobsApiClient.GetAllSkillsAsync();
                return this.View(dto);
            }

            this.AttachJwt();

            // Filter JobSkills to only include selected ones (where SkillId > 0)
            var selectedSkills = dto.JobSkills
                .Where(js => js.SkillId > 0 && js.RequiredPercentage > 0)
                .ToList();

            // Clear JobSkills from the DTO before sending to API
            // The skills will be handled separately via SkillLinks
            dto.JobSkills.Clear();

            AddJobDto addDto = new AddJobDto
            {
                JobPosting = dto,
                UserId = int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"),
                SkillLinks = selectedSkills,
            };
            Debug.WriteLine(addDto.UserId);

            bool success = await this.jobsApiClient.AddJobAsync(addDto);
            if (!success)
            {
                this.ModelState.AddModelError(string.Empty, "Failed to create job. Please try again.");
                this.ViewBag.Skills = await this.jobsApiClient.GetAllSkillsAsync();
                return this.View(dto);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Displays the edit job form. Recruiter only.
        /// </summary>
        /// <param name="id">The job posting ID.</param>
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Edit(int id)
        {
            this.AttachJwt();
            JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(id);
            if (job == null)
            {
                return this.NotFound();
            }

            this.ViewBag.Skills = await this.jobsApiClient.GetAllSkillsAsync();
            return this.View(job);
        }

        /// <summary>
        /// Handles edit job form submission. Recruiter only.
        /// </summary>
        /// <param name="id">The job posting ID.</param>
        /// <param name="dto">The updated job data.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Edit(int id, JobPostingDto dto)
        {
            if (!this.ModelState.IsValid)
            {
                this.AttachJwt();
                this.ViewBag.Skills = await this.jobsApiClient.GetAllSkillsAsync();
                return this.View(dto);
            }

            this.AttachJwt();
            bool success = await this.jobsApiClient.UpdateJobAsync(id, dto);
            if (!success)
            {
                this.ModelState.AddModelError(string.Empty, "Failed to update job. Please try again.");
                this.ViewBag.Skills = await this.jobsApiClient.GetAllSkillsAsync();
                return this.View(dto);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Displays the delete confirmation page. Recruiter only.
        /// </summary>
        /// <param name="id">The job posting ID.</param>
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Delete(int id)
        {
            this.AttachJwt();
            JobPostingDto? job = await this.jobsApiClient.GetJobByIdAsync(id);
            if (job == null)
            {
                return this.NotFound();
            }

            return this.View(job);
        }

        /// <summary>
        /// Handles confirmed deletion of a job posting. Recruiter only.
        /// </summary>
        /// <param name="id">The job posting ID.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            this.AttachJwt();
            await this.jobsApiClient.DeleteJobAsync(id);
            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Reads the JWT token from the current user's claims and attaches it to the API client.
        /// </summary>
        private void AttachJwt()
        {
            string? jwt = this.User.FindFirstValue("jwt");
            if (!string.IsNullOrEmpty(jwt))
            {
                this.jobsApiClient.SetAuthToken(jwt);
            }
        }
    }
}
