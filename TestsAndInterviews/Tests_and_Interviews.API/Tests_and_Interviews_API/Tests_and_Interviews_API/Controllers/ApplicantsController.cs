namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantsController : ControllerBase
    {
        private readonly IApplicantService _service;

        public ApplicantsController(IApplicantService service)
        {
            this._service = service;
        }

        [HttpGet("{applicantId}")]
        public ActionResult<ApplicantDto> GetById(int applicantId)
        {
            Applicant applicant = this._service.GetApplicantById(applicantId);

            if (applicant is null)
            {
                return NotFound($"Applicant with ID {applicantId} was not found.");
            }

            return Ok(applicant.ToDto());
        }

        [HttpGet("bycompany/{companyId}")]
        public ActionResult<List<ApplicantDto>> GetByCompany(int companyId)
        {
            IEnumerable<Applicant> applicants = this._service.GetApplicantsByCompany(companyId);

            if (applicants is null || !applicants.Any())
                return NotFound($"No applicants found for company ID {companyId}.");

            return Ok(applicants.Select(a => a.ToDto()).ToList());
        }

        [HttpGet("byjob/{jobId}")]
        public ActionResult<List<ApplicantDto>> GetByJob(int jobId)
        {
            JobPosting jobPosting = new JobPosting { JobId = jobId };
            IEnumerable<Applicant> applicants = this._service.GetApplicantsByJob(jobPosting);

            if (applicants is null || !applicants.Any())
                return NotFound($"No applicants found for job ID {jobId}.");

            return Ok(applicants.Select(a => a.ToDto()).ToList());
        }

        [HttpPost]
        public ActionResult<ApplicantDto> Add([FromBody] ApplicantDto dto)
        {
            Applicant applicant = dto.ToEntity();
            this._service.AddApplicant(applicant);

            return Ok(applicant.ToDto());
        }

        [HttpPut("{applicantId}")]
        public ActionResult<ApplicantDto> Update(int applicantId, [FromBody] ApplicantDto dto)
        {
            Applicant applicant = dto.ToEntity();
            applicant.ApplicantId = applicantId;
            this._service.UpdateApplicant(applicant);

            return Ok(applicant.ToDto());
        }

        [HttpDelete("{applicantId}")]
        public ActionResult Remove(int applicantId)
        {
            this._service.RemoveApplicant(applicantId);

            return Ok(new { message = "Applicant removed successfully" });
        }
    }
}