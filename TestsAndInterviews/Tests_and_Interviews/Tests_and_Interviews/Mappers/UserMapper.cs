namespace Tests_and_Interviews.Mappers
{
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Provides extension methods for mapping between User and UserDto objects.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Converts a User entity to its corresponding UserDto representation.
        /// </summary>
        /// <param name="entity">The User entity to convert. Cannot be null.</param>
        /// <returns>A UserDto object containing the data from the specified User entity.</returns>
        public static UserDto ToDto(this User entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                CvXml = entity.CvXml,
            };
        }

        /// <summary>
        /// Converts a UserDto instance to its corresponding User entity.
        /// </summary>
        /// <param name="dto">The UserDto object to convert. Cannot be null.</param>
        /// <returns>A new User entity populated with values from the specified UserDto.</returns>
        public static User ToEntity(this UserDto dto)
        {
            return new User(dto.Id, dto.Name, dto.Email, dto.CvXml);
        }
    }
}