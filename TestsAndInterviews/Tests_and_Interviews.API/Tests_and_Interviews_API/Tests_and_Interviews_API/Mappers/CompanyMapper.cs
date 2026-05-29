namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Provides extension methods for mapping between Company and CompanyDto objects.
    /// </summary>
    public static class CompanyMapper
    {
        /// <summary>
        /// Converts a Company entity to its corresponding CompanyDto representation.
        /// </summary>
        /// <param name="entity">The Company entity to convert. Cannot be null.</param>
        /// <returns>A CompanyDto object containing the data from the specified Company entity.</returns>
        public static CompanyDto ToDto(this Company entity)
        {
            return new CompanyDto
            {
                CompanyId = entity.CompanyId,
                Name = entity.Name,
                AboutUs = entity.AboutUs,
                ProfilePicturePath = entity.ProfilePicturePath,
                CompanyLogoPath = entity.CompanyLogoPath,
                Location = entity.Location,
                Email = entity.Email,
                PostedJobsCount = entity.PostedJobsCount,
                CollaboratorsCount = entity.CollaboratorsCount,
                BuddyName = entity.BuddyName,
                AvatarId = entity.AvatarId,
                FinalQuote = entity.FinalQuote,
                BuddyDescription = entity.BuddyDescription,
                Scen1Text = entity.Scen1Text,
                Scen1Answer1 = entity.Scen1Answer1,
                Scen1Answer2 = entity.Scen1Answer2,
                Scen1Answer3 = entity.Scen1Answer3,
                Scen1Reaction1 = entity.Scen1Reaction1,
                Scen1Reaction2 = entity.Scen1Reaction2,
                Scen1Reaction3 = entity.Scen1Reaction3,
                Scen2Text = entity.Scen2Text,
                Scen2Answer1 = entity.Scen2Answer1,
                Scen2Answer2 = entity.Scen2Answer2,
                Scen2Answer3 = entity.Scen2Answer3,
                Scen2Reaction1 = entity.Scen2Reaction1,
                Scen2Reaction2 = entity.Scen2Reaction2,
                Scen2Reaction3 = entity.Scen2Reaction3,
            };
        }

        /// <summary>
        /// Converts a CompanyDto instance to its corresponding Company entity.
        /// </summary>
        /// <param name="dto">The CompanyDto object to convert. Cannot be null.</param>
        /// <returns>A new Company entity populated with values from the specified CompanyDto.</returns>
        public static Company ToEntity(this CompanyDto dto)
        {
            return new Company
            {
                CompanyId = dto.CompanyId,
                Name = dto.Name,
                AboutUs = dto.AboutUs,
                ProfilePicturePath = dto.ProfilePicturePath,
                CompanyLogoPath = dto.CompanyLogoPath,
                Location = dto.Location,
                Email = dto.Email,
                PostedJobsCount = dto.PostedJobsCount,
                CollaboratorsCount = dto.CollaboratorsCount,
                BuddyName = dto.BuddyName,
                AvatarId = dto.AvatarId,
                FinalQuote = dto.FinalQuote,
                BuddyDescription = dto.BuddyDescription,
                Scen1Text = dto.Scen1Text,
                Scen1Answer1 = dto.Scen1Answer1,
                Scen1Answer2 = dto.Scen1Answer2,
                Scen1Answer3 = dto.Scen1Answer3,
                Scen1Reaction1 = dto.Scen1Reaction1,
                Scen1Reaction2 = dto.Scen1Reaction2,
                Scen1Reaction3 = dto.Scen1Reaction3,
                Scen2Text = dto.Scen2Text,
                Scen2Answer1 = dto.Scen2Answer1,
                Scen2Answer2 = dto.Scen2Answer2,
                Scen2Answer3 = dto.Scen2Answer3,
                Scen2Reaction1 = dto.Scen2Reaction1,
                Scen2Reaction2 = dto.Scen2Reaction2,
                Scen2Reaction3 = dto.Scen2Reaction3,
            };
        }
    }
}