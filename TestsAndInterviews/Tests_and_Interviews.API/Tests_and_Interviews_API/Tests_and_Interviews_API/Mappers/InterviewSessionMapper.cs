namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between InterviewSession and InterviewSessionDto objects.
    /// </summary>
    public static class InterviewSessionMapper
    {
        /// <summary>
        /// Converts the specified InterviewSession entity to an InterviewSessionDto instance.
        /// </summary>
        /// <remarks>If the entity's Video property is null or whitespace, the resulting DTO's Video
        /// property will be set to null regardless of the url parameter.</remarks>
        /// <param name="entity">The InterviewSession entity to convert. Cannot be null.</param>
        /// <param name="url">An optional URL to associate with the Video property in the resulting DTO. If null or whitespace, the Video
        /// property will be set to null.</param>
        /// <returns>An InterviewSessionDto instance representing the provided InterviewSession entity.</returns>
        public static InterviewSessionDto ToDto(this InterviewSession entity, HttpRequest request)
        {
            string? url = null;

            if (!string.IsNullOrWhiteSpace(entity.Video))
            {
                url = $"{request.Scheme}://{request.Host}/api/InterviewSessions/{entity.Video}";
            }

            return new InterviewSessionDto
            {
                Id = entity.Id,
                PositionId = entity.PositionId,
                ExternalUserId = entity.ExternalUserId,
                InterviewerId = entity.InterviewerId,
                DateStart = entity.DateStart,
                Video = url,
                Status = entity.Status,
                Score = entity.Score
            };
        }

        /// <summary>
        /// Converts an InterviewSessionDto instance to its corresponding InterviewSession entity.
        /// </summary>
        /// <param name="dto">The InterviewSessionDto object to convert. Cannot be null.</param>
        /// <returns>An InterviewSession entity populated with values from the specified InterviewSessionDto.</returns>
        public static InterviewSession ToEntity(this InterviewSessionDto dto)
        {
            return new InterviewSession
            {
                Id = dto.Id,
                PositionId = dto.PositionId,
                ExternalUserId = dto.ExternalUserId,
                InterviewerId = dto.InterviewerId,
                DateStart = dto.DateStart,
                Status = dto.Status,
                Score = dto.Score
            };
        }
    }
}
