using Tests_and_Interviews_API.Dtos;
using Tests_and_Interviews_API.Models.Core;

namespace Tests_and_Interviews.Mappers
{
    /// <summary>
    /// Provides extension methods for mapping between Question and QuestionDto objects.
    /// </summary>
    public static class QuestionMapper
    {
        /// <summary>
        /// Converts a Question entity to its corresponding QuestionDto representation.
        /// </summary>
        /// <param name="entity">The Question entity to convert. Cannot be null.</param>
        /// <returns>A QuestionDto object containing the data from the specified Question entity.</returns>
        public static QuestionDto ToDto(this Question entity)
        {
            return new QuestionDto
            {
                Id = entity.Id,
                QuestionText = entity.QuestionText,
                QuestionType = entity.QuestionTypeString,
                QuestionScore = entity.QuestionScore,
                QuestionAnswer = entity.QuestionAnswer,
                OptionsJson = entity.OptionsJson,
                TestId = entity.TestId
            };
        }

        /// <summary>
        /// Converts a QuestionDto instance to its corresponding Question entity.
        /// </summary>
        /// <param name="dto">The QuestionDto object to convert. Cannot be null.</param>
        /// <returns>A new Question entity populated with values from the specified QuestionDto.</returns>
        public static Question ToEntity(this QuestionDto dto)
        {
            return new Question
            {
                Id = dto.Id,
                QuestionText = dto.QuestionText,
                QuestionTypeString = dto.QuestionType,
                QuestionScore = dto.QuestionScore,
                QuestionAnswer = dto.QuestionAnswer,
                OptionsJson = dto.OptionsJson,
                TestId = dto.TestId
            };
        }
    }
}