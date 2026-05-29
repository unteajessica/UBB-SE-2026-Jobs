// <copyright file="DataProcessingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <inheritdoc cref="IDataProcessingService"/>
    public class DataProcessingService : IDataProcessingService
    {
        private readonly AppDbContext dbContext;
        private readonly ITestAttemptRepository attemptRepository;
        private readonly ITestRepository testRepository;

        public DataProcessingService(
            AppDbContext dbContext,
            ITestAttemptRepository attemptRepository,
            ITestRepository testRepository)
        {
            this.dbContext = dbContext;
            this.attemptRepository = attemptRepository;
            this.testRepository = testRepository;
        }

        /// <inheritdoc/>
        public async Task<bool> ProcessFinalizedAttemptAsync(int attemptId)
        {
            var attempt = await this.attemptRepository.FindByIdAsync(attemptId);

            if (attempt == null)
            {
                return false;
            }

            var validationError = await this.ValidateAttemptAsync(attempt);

            if (validationError != null)
            {
                attempt.IsValidated = false;
                attempt.PercentageScore = null;
                attempt.RejectionReason = validationError;
                attempt.RejectedAt = DateTime.UtcNow;

                await this.attemptRepository.UpdateAsync(attempt);
                return false;
            }

            attempt.IsValidated = true;
            attempt.PercentageScore = this.ConvertToPercentageScore(attempt.Score.GetValueOrDefault());
            attempt.RejectionReason = null;
            attempt.RejectedAt = null;

            await this.attemptRepository.UpdateAsync(attempt);
            return true;
        }

        /// <summary>
        /// Performs a series of business rule checks to ensure the attempt is eligible for processing.
        /// </summary>
        /// <param name="attempt">The <see cref="TestAttempt"/> entity to evaluate.</param>
        /// <returns>A string containing the rejection reason if invalid; otherwise, <c>null</c>.</returns>
        private async Task<string?> ValidateAttemptAsync(TestAttempt attempt)
        {
            if (attempt.ExternalUserId == null)
            {
                return "User does not exist.";
            }

            var user = await this.dbContext.Users.FindAsync(attempt.ExternalUserId.Value);
            if (user == null)
            {
                return "User does not exist.";
            }

            var test = await this.testRepository.FindByIdAsync(attempt.TestId);
            if (test == null)
            {
                return "Test does not exist.";
            }

            if (attempt.CompletedAt == null)
            {
                return "Attempt is incomplete. Missing completion time.";
            }

            if (string.IsNullOrWhiteSpace(attempt.Status))
            {
                return "Attempt status is missing.";
            }

            if (!string.Equals(attempt.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                return "Attempt is not eligible for leaderboard because status is not COMPLETED.";
            }

            if (attempt.Score < 0 || attempt.Score > 100)
            {
                return "Attempt score is invalid.";
            }

            if (!this.IsTestStillValidForLeaderboard(test))
            {
                return "Test is no longer valid for leaderboard inclusion.";
            }

            return null;
        }

        /// <summary>
        /// Determines if a test is still eligible for the leaderboard based on its creation date.
        /// Currently enforces a 3-month validity window.
        /// </summary>
        private bool IsTestStillValidForLeaderboard(Test test)
        {
            return test.CreatedAt.AddMonths(3) >= DateTime.UtcNow;
        }

        /// <summary>
        /// Normalizes the raw score into a percentage format.
        /// </summary>
        private decimal ConvertToPercentageScore(decimal originalScore)
        {
            return originalScore / 100m * 100m;
        }
    }
}