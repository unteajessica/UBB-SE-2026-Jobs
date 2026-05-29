// <copyright file="Company.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Models
{
    /// <summary>
    /// Represents a company participating in the recruitment process.
    /// </summary>
    public class CompanyPosting
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the job title of the position offered by the company.
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the recruiter associated with this company.
        /// </summary>
        public int RecruiterId { get; set; }
    }
}