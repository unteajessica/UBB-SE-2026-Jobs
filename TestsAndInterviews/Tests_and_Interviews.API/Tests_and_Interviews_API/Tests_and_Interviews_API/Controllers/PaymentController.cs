namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            this._service = service;
        }

        [HttpPost("process/{jobId}")]
        public async Task<ActionResult> ProcessPayment(int jobId, [FromQuery] int paymentAmount)
        {
            await this._service.ProcessPaymentAsync(jobId, paymentAmount);
            return Ok();
        }

        [HttpPut("{jobId}")]
        public ActionResult UpdateJobPayment(int jobId, [FromQuery] int paymentAmount)
        {
            this._service.UpdateJobPayment(jobId, paymentAmount);
            return Ok();
        }

        [HttpGet("paid")]
        public ActionResult<List<JobPaymentInfoDto>> GetPaidJobs([FromQuery] string jobType, [FromQuery] string experienceLevel)
        {
            List<JobPaymentInfo> jobs = this._service.GetPaidJobs(jobType, experienceLevel);

            if (jobs is null || !jobs.Any())
                return NotFound($"No paid jobs found for type '{jobType}' and experience level '{experienceLevel}'.");

            return Ok(jobs.Select(j => j.ToDto()).ToList());
        }
    }
}