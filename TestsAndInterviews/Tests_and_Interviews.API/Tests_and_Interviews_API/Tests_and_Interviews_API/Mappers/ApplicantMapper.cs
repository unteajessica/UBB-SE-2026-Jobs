namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Provides extension methods for mapping between Applicant and ApplicantDto objects.
    /// </summary>
    public static class ApplicantMapper
    {
        /// <summary>
        /// Converts an Applicant entity to its corresponding ApplicantDto representation.
        /// </summary>
        /// <param name="entity">The Applicant entity to convert. Cannot be null.</param>
        /// <returns>An ApplicantDto object containing the data from the specified Applicant entity.</returns>
        public static ApplicantDto ToDto(this Applicant entity)
        {
            return new ApplicantDto
            {
                ApplicantId = entity.ApplicantId,
                JobId = entity.JobId,
                UserId = entity.UserId,
                AppTestGrade = entity.AppTestGrade,
                CvGrade = entity.CvGrade,
                CompanyTestGrade = entity.CompanyTestGrade,
                InterviewGrade = entity.InterviewGrade,
                ApplicationStatus = entity.ApplicationStatus,
                AppliedAt = entity.AppliedAt,
                RecommendedFromCompanyId = entity.RecommendedFromCompanyId,
                CvFileUrl = entity.CvFileUrl,
            };
        }

        /// <summary>
        /// Converts an ApplicantDto instance to its corresponding Applicant entity.
        /// </summary>
        /// <param name="dto">The ApplicantDto object to convert. Cannot be null.</param>
        /// <returns>A new Applicant entity populated with values from the specified ApplicantDto.</returns>
        public static Applicant ToEntity(this ApplicantDto dto)
        {
            return new Applicant
            {
                ApplicantId = dto.ApplicantId,
                JobId = dto.JobId,
                UserId = dto.UserId,
                AppTestGrade = dto.AppTestGrade,
                CvGrade = dto.CvGrade,
                CompanyTestGrade = dto.CompanyTestGrade,
                InterviewGrade = dto.InterviewGrade,
                ApplicationStatus = dto.ApplicationStatus,
                AppliedAt = dto.AppliedAt,
                RecommendedFromCompanyId = dto.RecommendedFromCompanyId,
                CvFileUrl = dto.CvFileUrl,
            };
        }
    }
}