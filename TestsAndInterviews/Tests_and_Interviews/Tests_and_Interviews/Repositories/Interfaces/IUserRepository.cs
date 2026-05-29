namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// IUserRepository interface provides methods to perform CRUD on the Users
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their unique identifier. This method executes a SQL query to retrieve the user's details.
        /// </summary>
        /// <param name="id">The id of the user to be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all users asynchronously.
        /// This method executes a SQL query to retrieve all user records and maps them to a list of User objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<User>> GetAllAsync();

        /// <summary>
        /// Adds a new user.
        /// This method executes an INSERT SQL query to add a new user record and retrieves the generated ID for the newly inserted user.
        /// </summary>
        /// <param name="user">The user object containing the details of the user to be added.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddAsync(User user);

        /// <summary>
        /// Asynchronously updates an existing user's details.
        /// This method executes an UPDATE SQL query to modify the user's name and email based on their unique identifier (ID).
        /// </summary>
        /// <param name="user">The user object containing the details of the upated user</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// Asynchronously deletes a user based on their unique identifier (ID).
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteAsync(int id);
    }
}
