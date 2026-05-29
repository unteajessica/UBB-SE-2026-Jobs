// <copyright file="TestDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Web.Dtos
{
    using System;

    /// <summary>
    /// Represents a test or quiz that candidates can take.
    /// </summary>
    public class TestDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the test.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the test.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the test was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the label that describes the type of question.
        /// </summary>
        public string QuestionTypeLabel { get; set; } = "MIXED";
    }
}
