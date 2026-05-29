namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing applicants.
    /// </summary>
    public class ApplicantService : IApplicantService
    {
        private readonly IApplicantRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicantService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access applicant data. Cannot be null.</param>
        public ApplicantService(IApplicantRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Retrieves the applicant with the specified identifier.
        /// </summary>
        /// <param name="applicantId">The unique identifier of the applicant.</param>
        /// <returns>The applicant corresponding to the specified identifier.</returns>
        public Applicant GetApplicantById(int applicantId)
        {
            return this._repository.GetApplicantById(applicantId);
        }

        /// <summary>
        /// Retrieves all applicants associated with the specified company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A list of applicants associated with the specified company.</returns>
        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId)
        {
            return this._repository.GetApplicantsByCompany(companyId);
        }

        /// <summary>
        /// Retrieves all applicants associated with the specified job posting.
        /// </summary>
        /// <param name="jobPosting">The job posting to filter applicants by.</param>
        /// <returns>A list of applicants associated with the specified job posting.</returns>
        public IEnumerable<Applicant> GetApplicantsByJob(JobPosting jobPosting)
        {
            return this._repository.GetApplicantsByJob(jobPosting);
        }

        /// <summary>
        /// Adds a new applicant to the data store.
        /// </summary>
        /// <param name="applicant">The applicant to add. Cannot be null.</param>
        public void AddApplicant(Applicant applicant)
        {
            this._repository.AddApplicant(applicant);
        }

        /// <summary>
        /// Updates an existing applicant in the data store.
        /// </summary>
        /// <param name="applicant">The applicant with updated values. Cannot be null.</param>
        public void UpdateApplicant(Applicant applicant)
        {
            this._repository.UpdateApplicant(applicant);
        }

        /// <summary>
        /// Removes the applicant with the specified identifier from the data store.
        /// </summary>
        /// <param name="applicantId">The unique identifier of the applicant to remove.</param>
        public void RemoveApplicant(int applicantId)
        {
            this._repository.RemoveApplicant(applicantId);
        }
    }
}