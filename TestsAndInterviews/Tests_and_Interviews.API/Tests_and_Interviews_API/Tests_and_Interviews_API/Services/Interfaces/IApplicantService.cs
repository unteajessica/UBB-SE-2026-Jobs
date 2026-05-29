namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Defines operations for managing applicants.
    /// </summary>
    public interface IApplicantService
    {
        /// <summary>
        /// Retrieves the applicant with the specified identifier.
        /// </summary>
        /// <param name="applicantId">The unique identifier of the applicant.</param>
        /// <returns>The applicant corresponding to the specified identifier.</returns>
        Applicant GetApplicantById(int applicantId);

        /// <summary>
        /// Retrieves all applicants associated with the specified company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A list of applicants associated with the specified company.</returns>
        IEnumerable<Applicant> GetApplicantsByCompany(int companyId);

        /// <summary>
        /// Retrieves all applicants associated with the specified job posting.
        /// </summary>
        /// <param name="jobPosting">The job posting to filter applicants by.</param>
        /// <returns>A list of applicants associated with the specified job posting.</returns>
        IEnumerable<Applicant> GetApplicantsByJob(JobPosting jobPosting);

        /// <summary>
        /// Adds a new applicant to the data store.
        /// </summary>
        /// <param name="applicant">The applicant to add. Cannot be null.</param>
        void AddApplicant(Applicant applicant);

        /// <summary>
        /// Updates an existing applicant in the data store.
        /// </summary>
        /// <param name="applicant">The applicant with updated values. Cannot be null.</param>
        void UpdateApplicant(Applicant applicant);

        /// <summary>
        /// Removes the applicant with the specified identifier from the data store.
        /// </summary>
        /// <param name="applicantId">The unique identifier of the applicant to remove.</param>
        void RemoveApplicant(int applicantId);
    }
}