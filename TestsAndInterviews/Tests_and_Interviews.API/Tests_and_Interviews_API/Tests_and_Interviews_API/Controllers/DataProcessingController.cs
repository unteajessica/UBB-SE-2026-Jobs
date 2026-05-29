// <copyright file="DataProcessingController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews_API.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Handles data processing operations triggered by external clients (e.g. the Desktop app).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DataProcessingController : ControllerBase
    {
        private readonly IDataProcessingService dataProcessingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingController"/> class.
        /// </summary>
        /// <param name="dataProcessingService">The service that handles attempt finalization logic.</param>
        public DataProcessingController(IDataProcessingService dataProcessingService)
        {
            this.dataProcessingService = dataProcessingService;
        }

        /// <summary>
        /// Finalizes and validates a completed test attempt.
        /// </summary>
        /// <param name="attemptId">The ID of the test attempt to process.</param>
        /// <returns>200 OK if processed successfully; 422 if validation failed; 404 if not found.</returns>
        [HttpPost("finalize/{attemptId}")]
        public async Task<IActionResult> FinalizAttempt(int attemptId)
        {
            bool success = await this.dataProcessingService.ProcessFinalizedAttemptAsync(attemptId);

            if (!success)
            {
                return this.UnprocessableEntity(new { message = $"Attempt {attemptId} could not be processed or was rejected." });
            }

            return this.Ok(new { message = $"Attempt {attemptId} processed successfully." });
        }
    }
}