namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Provides extension methods for mapping between JobSkill and JobSkillDto objects.
    /// </summary>
    public static class JobSkillMapper
    {
        /// <summary>
        /// Converts a JobSkill entity to its corresponding JobSkillDto representation.
        /// </summary>
        /// <param name="entity">The JobSkill entity to convert. Cannot be null.</param>
        /// <returns>A JobSkillDto object containing the data from the specified JobSkill entity.</returns>
        public static JobSkillDto ToDto(this JobSkill entity)
        {
            return new JobSkillDto
            {
                SkillId = entity.SkillId,
                JobId = entity.JobId,
                RequiredPercentage = entity.RequiredPercentage,
                SkillDto = entity.Skill != null ? entity.Skill.ToDto() : null!,
            };
        }

        /// <summary>
        /// Converts a JobSkillDto instance to its corresponding JobSkill entity.
        /// </summary>
        /// <param name="dto">The JobSkillDto object to convert. Cannot be null.</param>
        /// <returns>A new JobSkill entity populated with values from the specified JobSkillDto.</returns>
        public static JobSkill ToEntity(this JobSkillDto dto)
        {
            return new JobSkill
            {
                SkillId = dto.SkillId,
                JobId = dto.JobId,
                RequiredPercentage = dto.RequiredPercentage,
                Skill = dto.SkillDto != null ? dto.SkillDto.ToEntity() : null!,
            };
        }
    }
}