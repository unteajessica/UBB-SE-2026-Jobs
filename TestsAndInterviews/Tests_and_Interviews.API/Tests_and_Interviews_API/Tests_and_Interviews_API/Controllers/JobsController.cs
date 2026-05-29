namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobsService _service;
        private readonly AppDbContext _dbContext;

        public JobsController(IJobsService service, AppDbContext dbContext)
        {
            this._service = service;
            this._dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<List<JobPostingDto>> GetAllJobs()
        {
            IEnumerable<JobPosting> jobs = this._service.GetAllJobs();

            return Ok(jobs.Select(j => j.ToDto()).ToList());
        }

        [HttpGet("skills")]
        public ActionResult<List<SkillDto>> GetAllSkills()
        {
            IReadOnlyList<Skill> skills = this._service.GetAllSkills();

            return Ok(skills.Select(s => s.ToDto()).ToList());
        }

        [HttpPost]
        public ActionResult<int> AddJob([FromBody] AddJobDto dto)
        {
            int userId = dto.UserId;
            if (userId <= 0)
            {
                return Unauthorized(new { message = "Unable to determine user identity." });
            }

            Recruiter? recruiter = this._dbContext.Recruiters.FirstOrDefault(r => r.UserId == userId);
            if (recruiter == null)
            {
                return Forbid("Recruiter profile not found for this user.");
            }

            JobPosting jobPosting = dto.JobPosting.ToEntity();
            IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks = dto.SkillLinks
                .Select(s => (s.SkillId, s.RequiredPercentage))
                .ToList();

            int jobId = this._service.AddJob(jobPosting, recruiter.CompanyId, skillLinks);

            return Ok(jobId);
        }

        [HttpPut("{jobId}")]
        public ActionResult UpdateJob(int jobId, [FromBody] JobPostingDto dto)
        {
            JobPosting? existingJob = this._service.GetJobById(jobId);
            if (existingJob == null)
            {
                return NotFound(new { message = $"Job with ID {jobId} not found." });
            }

            JobPosting updatedJob = dto.ToEntity();
            IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks = dto.JobSkills
                .Select(s => (s.SkillId, s.RequiredPercentage))
                .ToList();

            bool success = this._service.UpdateJob(jobId, updatedJob, skillLinks);
            if (!success)
            {
                return StatusCode(500, new { message = "An error occurred while updating the job." });
            }

            return NoContent();
        }

        [HttpDelete("{jobId}")]
        public ActionResult DeleteJob(int jobId)
        {
            JobPosting? existingJob = this._service.GetJobById(jobId);
            if (existingJob == null)
            {
                return NotFound(new { message = $"Job with ID {jobId} not found." });
            }

            bool success = this._service.DeleteJob(jobId);
            if (!success)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the job." });
            }

            return NoContent();
        }

        [HttpGet("{jobId}/skills")]
        public ActionResult<List<JobSkillDto>> GetSkillsByJob(int jobId)
        {
            IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks = this._service.GetSkillsByJob(jobId);

            if (skillLinks is null || !skillLinks.Any())
                return NotFound($"No skills found for job ID {jobId}.");

            return Ok(skillLinks.Select(s => new JobSkillDto
            {
                SkillId = s.SkillId,
                JobId = jobId,
                RequiredPercentage = s.RequiredPercentage,
            }).ToList());
        }
    }
}