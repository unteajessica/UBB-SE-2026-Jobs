namespace Tests_and_Interviews_API.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    public class CompanyRepo : ICompanyRepo
    {
        private readonly AppDbContext appDbContext;
        private Company? currentCompany;

        public CompanyRepo(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        private void ValidateRequiredFields(Company company)
        {
            if (company is null)
            {
                throw new ArgumentNullException(nameof(company));
            }
            if (string.IsNullOrWhiteSpace(company.Name))
            {
                throw new ArgumentException("Company name is required.", nameof(company));
            }
            if (string.IsNullOrWhiteSpace(company.CompanyLogoPath))
            {
                throw new ArgumentException("Company logo url/path is required.", nameof(company));
            }
        }

        private static Game MapGame(Company company)
        {
            var buddy = new Buddy(
                company.AvatarId ?? 0,
                company.BuddyName ?? string.Empty,
                company.BuddyDescription ?? string.Empty);

            var scenarios = new List<Scenario>();

            if (!string.IsNullOrEmpty(company.Scen1Text))
            {
                var scenario1 = new Scenario(company.Scen1Text);
                scenario1.AddChoice(new AdviceChoice(company.Scen1Answer1 ?? string.Empty, company.Scen1Reaction1 ?? string.Empty));
                scenario1.AddChoice(new AdviceChoice(company.Scen1Answer2 ?? string.Empty, company.Scen1Reaction2 ?? string.Empty));
                scenario1.AddChoice(new AdviceChoice(company.Scen1Answer3 ?? string.Empty, company.Scen1Reaction3 ?? string.Empty));
                scenarios.Add(scenario1);
            }

            if (!string.IsNullOrEmpty(company.Scen2Text))
            {
                var scenario2 = new Scenario(company.Scen2Text);
                scenario2.AddChoice(new AdviceChoice(company.Scen2Answer1 ?? string.Empty, company.Scen2Reaction1 ?? string.Empty));
                scenario2.AddChoice(new AdviceChoice(company.Scen2Answer2 ?? string.Empty, company.Scen2Reaction2 ?? string.Empty));
                scenario2.AddChoice(new AdviceChoice(company.Scen2Answer3 ?? string.Empty, company.Scen2Reaction3 ?? string.Empty));
                scenarios.Add(scenario2);
            }

            return new Game(buddy, scenarios, company.FinalQuote ?? string.Empty, true);
        }

        public Game? GetGame()
        {
            if (this.currentCompany == null)
            {
                return null;
            }

            return this.currentCompany.Game;
        }

        public void SaveGame(Game game)
        {
            if (this.currentCompany == null)
            {
                throw new InvalidOperationException("Nu exista o companie curenta selectata.");
            }

            this.currentCompany.Game = game;

            var existing = this.appDbContext.Companies.Find(this.currentCompany.CompanyId);
            if (existing == null)
            {
                return;
            }

            existing.BuddyName = game.Buddy.Name;
            existing.BuddyDescription = game.Buddy.Introduction;
            existing.AvatarId = game.Buddy.Id;
            existing.FinalQuote = game.Conclusion;
            existing.Scen1Text = game.GetScenario(0).Description;
            existing.Scen1Answer1 = game.GetScenario(0).GetAdviceTexts()[0];
            existing.Scen1Answer2 = game.GetScenario(0).GetAdviceTexts()[1];
            existing.Scen1Answer3 = game.GetScenario(0).GetAdviceTexts()[2];
            existing.Scen1Reaction1 = game.GetScenario(0).GetAdviceReactions()[0];
            existing.Scen1Reaction2 = game.GetScenario(0).GetAdviceReactions()[1];
            existing.Scen1Reaction3 = game.GetScenario(0).GetAdviceReactions()[2];
            existing.Scen2Text = game.GetScenario(1).Description;
            existing.Scen2Answer1 = game.GetScenario(1).GetAdviceTexts()[0];
            existing.Scen2Answer2 = game.GetScenario(1).GetAdviceTexts()[1];
            existing.Scen2Answer3 = game.GetScenario(1).GetAdviceTexts()[2];
            existing.Scen2Reaction1 = game.GetScenario(1).GetAdviceReactions()[0];
            existing.Scen2Reaction2 = game.GetScenario(1).GetAdviceReactions()[1];
            existing.Scen2Reaction3 = game.GetScenario(1).GetAdviceReactions()[2];

            this.appDbContext.SaveChanges();
        }

        public void PrintAll()
        {
            var companies = this.appDbContext.Companies.ToList();
            foreach (var company in companies)
            {
                System.Diagnostics.Debug.WriteLine($"{company} ");
            }
        }

        ObservableCollection<Company> ICompanyRepo.GetAll()
        {
            var companies = this.appDbContext.Companies.ToList();
            foreach (var company in companies)
            {
                company.Game = MapGame(company);
            }

            return new ObservableCollection<Company>(companies);
        }

        Company? ICompanyRepo.GetById(int companyId)
        {
            var company = this.appDbContext.Companies
                .FirstOrDefault(c => c.CompanyId == companyId);

            if (company == null)
            {
                return null;
            }

            company.Game = MapGame(company);
            this.currentCompany = company;
            return company;
        }

        void ICompanyRepo.Add(Company company)
        {
            ValidateRequiredFields(company);

            this.appDbContext.Companies.Add(company);
            this.appDbContext.SaveChanges();
        }

        void ICompanyRepo.Remove(int companyId)
        {
            var company = this.appDbContext.Companies.Find(companyId);
            if (company != null)
            {
                this.appDbContext.Companies.Remove(company);
                this.appDbContext.SaveChanges();
            }
        }

        void ICompanyRepo.Update(Company company)
        {
            ValidateRequiredFields(company);

            var existing = this.appDbContext.Companies.Find(company.CompanyId);
            if (existing == null)
            {
                throw new InvalidOperationException($"No company found with id '{company.CompanyId}' to update.");
            }

            existing.Name = company.Name;
            existing.AboutUs = company.AboutUs;
            existing.ProfilePicturePath = company.ProfilePicturePath;
            existing.CompanyLogoPath = company.CompanyLogoPath;
            existing.Location = company.Location;
            existing.Email = company.Email;
            existing.BuddyName = company.BuddyName;
            existing.BuddyDescription = company.BuddyDescription;
            existing.AvatarId = company.AvatarId;
            existing.FinalQuote = company.FinalQuote;
            existing.Scen1Text = company.Scen1Text;
            existing.Scen1Answer1 = company.Scen1Answer1;
            existing.Scen1Answer2 = company.Scen1Answer2;
            existing.Scen1Answer3 = company.Scen1Answer3;
            existing.Scen1Reaction1 = company.Scen1Reaction1;
            existing.Scen1Reaction2 = company.Scen1Reaction2;
            existing.Scen1Reaction3 = company.Scen1Reaction3;
            existing.Scen2Text = company.Scen2Text;
            existing.Scen2Answer1 = company.Scen2Answer1;
            existing.Scen2Answer2 = company.Scen2Answer2;
            existing.Scen2Answer3 = company.Scen2Answer3;
            existing.Scen2Reaction1 = company.Scen2Reaction1;
            existing.Scen2Reaction2 = company.Scen2Reaction2;
            existing.Scen2Reaction3 = company.Scen2Reaction3;

            this.appDbContext.SaveChanges();
        }

        public Company? GetCompanyByName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return null;
            }

            var company = this.appDbContext.Companies
                .FirstOrDefault(c => c.Name == companyName);

            if (company != null)
            {
                company.Game = MapGame(company);
            }

            return company;
        }
    }
}