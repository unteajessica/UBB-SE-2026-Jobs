namespace Tests_and_Interviews.Mappers
{
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between TestAttempt and TestAttemptDto objects.
    /// </summary>
    public static class TestAttemptMapper
    {
        /// <summary>
        /// Converts a TestAttempt entity to its corresponding TestAttemptDto representation.
        /// </summary>
        /// <param name="entity">The TestAttempt entity to convert. Cannot be null.</param>
        /// <returns>A TestAttemptDto object containing the data from the specified TestAttempt entity.</returns>
        public static TestAttemptDto ToDto(this TestAttempt entity)
        {
            return new TestAttemptDto
            {
                Id = entity.Id,
                TestId = entity.TestId,
                ExternalUserId = entity.ExternalUserId,
                Score = entity.Score,
                Status = entity.Status,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                AnswersFilePath = entity.AnswersFilePath,
                IsValidated = entity.IsValidated,
                PercentageScore = entity.PercentageScore,
                RejectionReason = entity.RejectionReason,
                RejectedAt = entity.RejectedAt,
            };
        }

        /// <summary>
        /// Converts a TestAttemptDto instance to its corresponding TestAttempt entity.
        /// </summary>
        /// <param name="dto">The TestAttemptDto object to convert. Cannot be null.</param>
        /// <returns>A new TestAttempt entity populated with values from the specified TestAttemptDto.</returns>
        public static TestAttempt ToEntity(this TestAttemptDto dto)
        {
            return new TestAttempt
            {
                Id = dto.Id,
                TestId = dto.TestId,
                ExternalUserId = dto.ExternalUserId,
                Score = dto.Score,
                Status = dto.Status,
                StartedAt = dto.StartedAt,
                CompletedAt = dto.CompletedAt,
                AnswersFilePath = dto.AnswersFilePath,
                IsValidated = dto.IsValidated,
                PercentageScore = dto.PercentageScore,
                RejectionReason = dto.RejectionReason,
                RejectedAt = dto.RejectedAt,
            };
        }
    }
}