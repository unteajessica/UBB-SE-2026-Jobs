namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines operations for retrieving company statistics.
    /// </summary>
    public interface ICompanyStatsService
    {
        /// <summary>
        /// Retrieves the top 3 skills required across all jobs posted by the specified company,
        /// along with their relative percentage of total required skill weight.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A task containing a tuple of skill names and their corresponding percentages.</returns>
        Task<(List<string> skillNames, List<int> percents)> GetSkillsTop3Async(int companyId);

        /// <summary>
        /// Generates a message describing applicant trends for the specified company
        /// by comparing the current week's applicants to the previous week's.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A task containing a human-readable applicant trend message.</returns>
        Task<string> ApplicantsMessageAsync(int companyId);
    }
}