namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing companies.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepo _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access company data. Cannot be null.</param>
        public CompanyService(ICompanyRepo repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Retrieves all companies.
        /// </summary>
        /// <returns>A list of all companies.</returns>
        public List<Company> GetAll()
        {
            return this._repository.GetAll().ToList();
        }

        /// <summary>
        /// Retrieves the company with the specified identifier.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>The company corresponding to the specified identifier, or null if not found.</returns>
        public Company? GetById(int companyId)
        {
            return this._repository.GetById(companyId);
        }

        /// <summary>
        /// Retrieves the company with the specified name.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <returns>The company corresponding to the specified name, or null if not found.</returns>
        public Company? GetCompanyByName(string companyName)
        {
            return this._repository.GetCompanyByName(companyName);
        }

        /// <summary>
        /// Adds a new company to the data store.
        /// </summary>
        /// <param name="company">The company to add. Cannot be null.</param>
        public void Add(Company company)
        {
            this._repository.Add(company);
        }

        /// <summary>
        /// Updates an existing company in the data store.
        /// </summary>
        /// <param name="company">The company with updated values. Cannot be null.</param>
        public void Update(Company company)
        {
            this._repository.Update(company);
        }

        /// <summary>
        /// Removes the company with the specified identifier from the data store.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company to remove.</param>
        public void Remove(int companyId)
        {
            this._repository.Remove(companyId);
        }

        /// <summary>
        /// Retrieves the game associated with the specified company.
        /// </summary>
        public GameDto? GetGame(int companyId)
        {
            Company? company = this._repository.GetById(companyId);
            if (company == null)
            {
                return null;
            }
            return new GameDto
            {
                AvatarId = company.AvatarId ?? 0,
                BuddyName = company.BuddyName ?? string.Empty,
                BuddyDescription = company.BuddyDescription ?? string.Empty,
                FinalQuote = company.FinalQuote ?? string.Empty,
                Scen1Text = company.Scen1Text ?? string.Empty,
                Scen1Answer1 = company.Scen1Answer1 ?? string.Empty,
                Scen1Answer2 = company.Scen1Answer2 ?? string.Empty,
                Scen1Answer3 = company.Scen1Answer3 ?? string.Empty,
                Scen1Reaction1 = company.Scen1Reaction1 ?? string.Empty,
                Scen1Reaction2 = company.Scen1Reaction2 ?? string.Empty,
                Scen1Reaction3 = company.Scen1Reaction3 ?? string.Empty,
                Scen2Text = company.Scen2Text ?? string.Empty,
                Scen2Answer1 = company.Scen2Answer1 ?? string.Empty,
                Scen2Answer2 = company.Scen2Answer2 ?? string.Empty,
                Scen2Answer3 = company.Scen2Answer3 ?? string.Empty,
                Scen2Reaction1 = company.Scen2Reaction1 ?? string.Empty,
                Scen2Reaction2 = company.Scen2Reaction2 ?? string.Empty,
                Scen2Reaction3 = company.Scen2Reaction3 ?? string.Empty,
                IsPublished = true,
            };
        }

        /// <summary>
        /// Saves the game associated with the specified company.
        /// </summary>
        public void SaveGame(int companyId, GameDto gameDto)
        {
            Company? company = this._repository.GetById(companyId);
            if (company == null)
            {
                throw new InvalidOperationException($"No company found with id '{companyId}'.");
            }
            company.AvatarId = gameDto.AvatarId;
            company.BuddyName = gameDto.BuddyName;
            company.BuddyDescription = gameDto.BuddyDescription;
            company.FinalQuote = gameDto.FinalQuote;
            company.Scen1Text = gameDto.Scen1Text;
            company.Scen1Answer1 = gameDto.Scen1Answer1;
            company.Scen1Answer2 = gameDto.Scen1Answer2;
            company.Scen1Answer3 = gameDto.Scen1Answer3;
            company.Scen1Reaction1 = gameDto.Scen1Reaction1;
            company.Scen1Reaction2 = gameDto.Scen1Reaction2;
            company.Scen1Reaction3 = gameDto.Scen1Reaction3;
            company.Scen2Text = gameDto.Scen2Text;
            company.Scen2Answer1 = gameDto.Scen2Answer1;
            company.Scen2Answer2 = gameDto.Scen2Answer2;
            company.Scen2Answer3 = gameDto.Scen2Answer3;
            company.Scen2Reaction1 = gameDto.Scen2Reaction1;
            company.Scen2Reaction2 = gameDto.Scen2Reaction2;
            company.Scen2Reaction3 = gameDto.Scen2Reaction3;
            this._repository.Update(company);
        }
    }
}