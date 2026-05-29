namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing collaborators.
    /// </summary>
    public class CollaboratorsService : ICollaboratorsService
    {
        private readonly ICollaboratorsRepo _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaboratorsService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access collaborator data. Cannot be null.</param>
        public CollaboratorsService(ICollaboratorsRepo repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Adds a collaborator to the specified event.
        /// </summary>
        /// <param name="eventOfCollaboration">The event to which the collaborator will be added.</param>
        /// <param name="collaboratorToBeAdded">The company to be added as a collaborator.</param>
        /// <param name="loggedInUserID">The unique identifier of the logged in user.</param>
        public void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID)
        {
            this._repository.AddCollaboratorToRepo(eventOfCollaboration, collaboratorToBeAdded, loggedInUserID);
        }

        /// <summary>
        /// Retrieves all collaborators associated with the specified company.
        /// </summary>
        /// <param name="loggedInCompanyId">The unique identifier of the logged in company.</param>
        /// <returns>A list of companies that are collaborators of the specified company.</returns>
        public List<Company> GetAllCollaborators(int loggedInCompanyId)
        {
            return this._repository.GetAllCollaborators(loggedInCompanyId);
        }
    }
}