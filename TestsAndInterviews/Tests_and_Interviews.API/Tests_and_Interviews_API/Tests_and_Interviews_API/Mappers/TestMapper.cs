// <copyright file="TestMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between Test and TestDto objects.
    /// </summary>
    public static class TestMapper
    {
        /// <summary>
        /// Converts a Test entity to its corresponding TestDto representation.
        /// </summary>
        /// <param name="entity">The Test entity to convert. Cannot be null.</param>
        /// <returns>A TestDto object containing the data from the specified Test entity.</returns>
        public static TestDto ToDto(this Test entity)
        {
            return new TestDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Category = entity.Category,
                CreatedAt = entity.CreatedAt,
                QuestionTypeLabel = entity.Questions?.FirstOrDefault()?.QuestionTypeString ?? "MIXED"
            };
        }

        /// <summary>
        /// Converts a TestDto instance to its corresponding Test entity.
        /// </summary>
        /// <param name="dto">The TestDto object to convert. Cannot be null.</param>
        /// <returns>A new Test entity populated with values from the specified TestDto.</returns>
        public static Test ToEntity(this TestDto dto)
        {
            return new Test
            {
                Id = dto.Id,
                Title = dto.Title,
                Category = dto.Category,
                CreatedAt = dto.CreatedAt,
            };
        }
    }
}