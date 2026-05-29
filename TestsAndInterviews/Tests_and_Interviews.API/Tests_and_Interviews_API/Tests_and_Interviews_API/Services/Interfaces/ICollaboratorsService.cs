namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Defines operations for managing collaborators.
    /// </summary>
    public interface ICollaboratorsService
    {
        /// <summary>
        /// Adds a collaborator to the specified event.
        /// </summary>
        /// <param name="eventOfCollaboration">The event to which the collaborator will be added.</param>
        /// <param name="collaboratorToBeAdded">The company to be added as a collaborator.</param>
        /// <param name="loggedInUserID">The unique identifier of the logged in user.</param>
        void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID);

        /// <summary>
        /// Retrieves all collaborators associated with the specified company.
        /// </summary>
        /// <param name="loggedInCompanyId">The unique identifier of the logged in company.</param>
        /// <returns>A list of companies that are collaborators of the specified company.</returns>
        List<Company> GetAllCollaborators(int loggedInCompanyId);
    }
}