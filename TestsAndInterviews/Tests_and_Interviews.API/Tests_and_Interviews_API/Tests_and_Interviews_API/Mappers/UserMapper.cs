namespace Tests_and_Interviews_API.Mappers
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models.Core;

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
                PasswordHash = entity.PasswordHash,
                Role = entity.Role,
            };
        }

        /// <summary>
        /// Converts a UserDto instance to its corresponding User entity.
        /// </summary>
        /// <param name="dto">The UserDto object to convert. Cannot be null.</param>
        /// <returns>A new User entity populated with values from the specified UserDto.</returns>
        public static User ToEntity(this UserDto dto)
        {
            var parts = dto.Name.Split(' ', 2);
            var firstName = parts.Length > 0 ? parts[0] : dto.Name;
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;
            return new User(dto.Id, firstName, lastName, dto.Email, dto.PasswordHash, dto.Role, dto.CvXml);
        }
    }
}