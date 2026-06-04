namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Defines operations for managing companies.
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Retrieves all companies.
        /// </summary>
        /// <returns>A list of all companies.</returns>
        List<Company> GetAll();

        /// <summary>
        /// Retrieves the company with the specified identifier.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>The company corresponding to the specified identifier, or null if not found.</returns>
        Company? GetById(int companyId);

        /// <summary>
        /// Retrieves the company with the specified name.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <returns>The company corresponding to the specified name, or null if not found.</returns>
        Company? GetCompanyByName(string companyName);
        
        /// <summary>
        /// Retrieves all the companies the recruiter is associated with.
        /// </summary>
        /// <param name="recruiterId">Id of recruiter to search companies.</param>
        /// <returns><List of the companies/returns>
        List<Company> GetByRecruiter(int recruiterId);

        /// <summary>
        /// Adds a new company to the data store.
        /// </summary>
        /// <param name="company">The company to add. Cannot be null.</param>
        void Add(Company company);

        /// <summary>
        /// Updates an existing company in the data store.
        /// </summary>
        /// <param name="company">The company with updated values. Cannot be null.</param>
        void Update(Company company);

        /// <summary>
        /// Removes the company with the specified identifier from the data store.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company to remove.</param>
        void Remove(int companyId);

        /// <summary>
        /// Retrieves the game associated with the specified company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>The game DTO for the company, or null if not found.</returns>
        GameDto? GetGame(int companyId);

        /// <summary>
        /// Saves the game associated with the specified company.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="gameDto">The game data to save.</param>
        void SaveGame(int companyId, GameDto gameDto);
    }
}