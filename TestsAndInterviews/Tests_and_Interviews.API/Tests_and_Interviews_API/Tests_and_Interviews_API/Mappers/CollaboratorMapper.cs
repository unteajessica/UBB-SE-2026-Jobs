namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Provides extension methods for mapping between Collaborator and CollaboratorDto objects.
    /// </summary>
    public static class CollaboratorMapper
    {
        /// <summary>
        /// Converts a Collaborator entity to its corresponding CollaboratorDto representation.
        /// </summary>
        /// <param name="entity">The Collaborator entity to convert. Cannot be null.</param>
        /// <returns>A CollaboratorDto object containing the data from the specified Collaborator entity.</returns>
        public static CollaboratorDto ToDto(this Collaborator entity)
        {
            return new CollaboratorDto
            {
                EventId = entity.EventId,
                CompanyId = entity.CompanyId,
            };
        }

        /// <summary>
        /// Converts a CollaboratorDto instance to its corresponding Collaborator entity.
        /// </summary>
        /// <param name="dto">The CollaboratorDto object to convert. Cannot be null.</param>
        /// <returns>A new Collaborator entity populated with values from the specified CollaboratorDto.</returns>
        public static Collaborator ToEntity(this CollaboratorDto dto)
        {
            return new Collaborator
            {
                EventId = dto.EventId,
                CompanyId = dto.CompanyId,
            };
        }
    }
}