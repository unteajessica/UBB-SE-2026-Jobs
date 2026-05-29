namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class CompanyStatsController : ControllerBase
    {
        private readonly ICompanyStatsService _service;

        public CompanyStatsController(ICompanyStatsService service)
        {
            this._service = service;
        }

        [HttpGet("{companyId}/skills/top3")]
        public async Task<ActionResult> GetSkillsTop3(int companyId)
        {
            var (skillNames, percents) = await this._service.GetSkillsTop3Async(companyId);

            if (skillNames == null || skillNames.Count == 0)
                return NotFound($"No skills found for company ID {companyId}.");

            return Ok(new { SkillNames = skillNames, Percents = percents });
        }

        [HttpGet("{companyId}/applicantsmessage")]
        public async Task<ActionResult<string>> GetApplicantsMessage(int companyId)
        {
            string message = await this._service.ApplicantsMessageAsync(companyId);
            return Ok(message);
        }
    }
}