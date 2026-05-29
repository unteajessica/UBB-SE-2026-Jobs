namespace Tests_and_Interviews.Mappers
{
    using System.Linq;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models;

    /// <summary>
    /// Provides extension methods for mapping between JobPosting and JobPostingDto objects.
    /// </summary>
    public static class JobPostingMapper
    {
        /// <summary>
        /// Converts a JobPosting entity to its corresponding JobPostingDto representation.
        /// </summary>
        /// <param name="entity">The JobPosting entity to convert. Cannot be null.</param>
        /// <returns>A JobPostingDto object containing the data from the specified JobPosting entity.</returns>
        public static JobPostingDto ToDto(this JobPosting entity)
        {
            return new JobPostingDto
            {
                JobId = entity.JobId,
                CompanyId = entity.CompanyId,
                Photo = entity.Photo,
                JobTitle = entity.JobTitle,
                IndustryField = entity.IndustryField,
                JobType = entity.JobType,
                ExperienceLevel = entity.ExperienceLevel,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                JobDescription = entity.JobDescription,
                JobLocation = entity.JobLocation,
                AvailablePositions = entity.AvailablePositions,
                PostedAt = entity.PostedAt,
                Salary = entity.Salary,
                AmountPayed = entity.AmountPayed,
                Deadline = entity.Deadline,
                JobSkills = entity.JobSkills.Select(js => js.ToDto()).ToList(),
            };
        }

        /// <summary>
        /// Converts a JobPostingDto instance to its corresponding JobPosting entity.
        /// </summary>
        /// <param name="dto">The JobPostingDto object to convert. Cannot be null.</param>
        /// <returns>A new JobPosting entity populated with values from the specified JobPostingDto.</returns>
        public static JobPosting ToEntity(this JobPostingDto dto)
        {
            return new JobPosting
            {
                JobId = dto.JobId,
                CompanyId = dto.CompanyId,
                Photo = dto.Photo,
                JobTitle = dto.JobTitle,
                IndustryField = dto.IndustryField,
                JobType = dto.JobType,
                ExperienceLevel = dto.ExperienceLevel,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                JobDescription = dto.JobDescription,
                JobLocation = dto.JobLocation,
                AvailablePositions = dto.AvailablePositions,
                PostedAt = dto.PostedAt,
                Salary = dto.Salary,
                AmountPayed = dto.AmountPayed,
                Deadline = dto.Deadline,
                JobSkills = dto.JobSkills.Select(js => js.ToEntity()).ToList(),
            };
        }
    }
}