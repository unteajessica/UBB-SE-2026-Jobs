namespace Tests_and_Interviews.Mappers
{
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between Answer and AnswerDto objects.
    /// </summary>
    public static class AnswerMapper
    {
        /// <summary>
        /// Converts an Answer entity to its corresponding AnswerDto representation.
        /// </summary>
        /// <param name="entity">The Answer entity to convert. Cannot be null.</param>
        /// <returns>An AnswerDto object containing the data from the specified Answer entity.</returns>
        public static AnswerDto ToDto(this Answer entity)
        {
            return new AnswerDto
            {
                QuestionId = entity.QuestionId,
                Value = entity.Value,
                AttemptId = entity.AttemptId,
                Question = entity.Question?.ToDto()
            };
        }

        /// <summary>
        /// Converts an AnswerDto instance to its corresponding Answer entity.
        /// </summary>
        /// <param name="dto">The AnswerDto object to convert. Cannot be null.</param>
        /// <returns>A new Answer entity populated with values from the specified AnswerDto.</returns>
        public static Answer ToEntity(this AnswerDto dto)
        {
            return new Answer
            {
                QuestionId = dto.QuestionId,
                Value = dto.Value,
                AttemptId = dto.AttemptId,
                Question = dto.Question?.ToEntity()
            };
        }
    }
}