namespace Tests_and_Interviews.Mappers
{
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between LeaderboardEntry and LeaderboardEntryDto objects.
    /// </summary>
    public static class LeaderboardEntryMapper
    {
        /// <summary>
        /// Converts a LeaderboardEntry entity to its corresponding LeaderboardEntryDto representation.
        /// </summary>
        /// <param name="entity">The LeaderboardEntry entity to convert. Cannot be null.</param>
        /// <returns>A LeaderboardEntryDto object containing the data from the specified LeaderboardEntry entity.</returns>
        public static LeaderboardEntryDto ToDto(this LeaderboardEntry entity)
        {
            return new LeaderboardEntryDto
            {
                Id = entity.Id,
                TestId = entity.TestId,
                UserId = entity.UserId,
                NormalizedScore = entity.NormalizedScore,
                RankPosition = entity.RankPosition,
                TieBreakPriority = entity.TieBreakPriority,
                LastRecalculationAt = entity.LastRecalculationAt,
            };
        }

        /// <summary>
        /// Converts a LeaderboardEntryDto instance to its corresponding LeaderboardEntry entity.
        /// </summary>
        /// <param name="dto">The LeaderboardEntryDto object to convert. Cannot be null.</param>
        /// <returns>A new LeaderboardEntry entity populated with values from the specified LeaderboardEntryDto.</returns>
        public static LeaderboardEntry ToEntity(this LeaderboardEntryDto dto)
        {
            return new LeaderboardEntry
            {
                Id = dto.Id,
                TestId = dto.TestId,
                UserId = dto.UserId,
                NormalizedScore = dto.NormalizedScore,
                RankPosition = dto.RankPosition,
                TieBreakPriority = dto.TieBreakPriority,
                LastRecalculationAt = dto.LastRecalculationAt,
            };
        }
    }
}