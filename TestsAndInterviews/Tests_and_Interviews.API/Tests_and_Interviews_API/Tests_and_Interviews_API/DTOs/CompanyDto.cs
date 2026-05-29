namespace Tests_and_Interviews_API.Dtos
{
    /// <summary>
    /// Represents a company entity.
    /// </summary>
    public class CompanyDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the company.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the about us section of the company.
        /// </summary>
        public string? AboutUs { get; set; }

        /// <summary>
        /// Gets or sets the profile picture path of the company.
        /// </summary>
        public string? ProfilePicturePath { get; set; }

        /// <summary>
        /// Gets or sets the logo path of the company.
        /// </summary>
        public string CompanyLogoPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location of the company.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the email address of the company.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the number of jobs posted by the company.
        /// </summary>
        public int PostedJobsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of collaborators associated with the company.
        /// </summary>
        public int CollaboratorsCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the company's buddy character.
        /// </summary>
        public string? BuddyName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the avatar associated with the company's buddy character.
        /// </summary>
        public int? AvatarId { get; set; }

        /// <summary>
        /// Gets or sets the final quote associated with the company's buddy character.
        /// </summary>
        public string? FinalQuote { get; set; }

        /// <summary>
        /// Gets or sets the description of the company's buddy character.
        /// </summary>
        public string? BuddyDescription { get; set; }

        /// <summary>
        /// Gets or sets the text for the first scenario.
        /// </summary>
        public string? Scen1Text { get; set; }

        /// <summary>
        /// Gets or sets the first answer option for the first scenario.
        /// </summary>
        public string? Scen1Answer1 { get; set; }

        /// <summary>
        /// Gets or sets the second answer option for the first scenario.
        /// </summary>
        public string? Scen1Answer2 { get; set; }

        /// <summary>
        /// Gets or sets the third answer option for the first scenario.
        /// </summary>
        public string? Scen1Answer3 { get; set; }

        /// <summary>
        /// Gets or sets the first reaction option for the first scenario.
        /// </summary>
        public string? Scen1Reaction1 { get; set; }

        /// <summary>
        /// Gets or sets the second reaction option for the first scenario.
        /// </summary>
        public string? Scen1Reaction2 { get; set; }

        /// <summary>
        /// Gets or sets the third reaction option for the first scenario.
        /// </summary>
        public string? Scen1Reaction3 { get; set; }

        /// <summary>
        /// Gets or sets the text for the second scenario.
        /// </summary>
        public string? Scen2Text { get; set; }

        /// <summary>
        /// Gets or sets the first answer option for the second scenario.
        /// </summary>
        public string? Scen2Answer1 { get; set; }

        /// <summary>
        /// Gets or sets the second answer option for the second scenario.
        /// </summary>
        public string? Scen2Answer2 { get; set; }

        /// <summary>
        /// Gets or sets the third answer option for the second scenario.
        /// </summary>
        public string? Scen2Answer3 { get; set; }

        /// <summary>
        /// Gets or sets the first reaction option for the second scenario.
        /// </summary>
        public string? Scen2Reaction1 { get; set; }

        /// <summary>
        /// Gets or sets the second reaction option for the second scenario.
        /// </summary>
        public string? Scen2Reaction2 { get; set; }

        /// <summary>
        /// Gets or sets the third reaction option for the second scenario.
        /// </summary>
        public string? Scen2Reaction3 { get; set; }
    }
}