namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Provides extension methods for mapping between Skill and SkillDto objects.
    /// </summary>
    public static class SkillMapper
    {
        /// <summary>
        /// Converts a Skill entity to its corresponding SkillDto representation.
        /// </summary>
        /// <param name="entity">The Skill entity to convert. Cannot be null.</param>
        /// <returns>A SkillDto object containing the data from the specified Skill entity.</returns>
        public static SkillDto ToDto(this Skill entity)
        {
            return new SkillDto
            {
                SkillId = entity.SkillId,
                SkillName = entity.SkillName,
            };
        }

        /// <summary>
        /// Converts a SkillDto instance to its corresponding Skill entity.
        /// </summary>
        /// <param name="dto">The SkillDto object to convert. Cannot be null.</param>
        /// <returns>A new Skill entity populated with values from the specified SkillDto.</returns>
        public static Skill ToEntity(this SkillDto dto)
        {
            return new Skill
            {
                SkillId = dto.SkillId,
                SkillName = dto.SkillName,
            };
        }
    }
}