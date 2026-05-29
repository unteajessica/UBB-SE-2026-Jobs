namespace Tests_and_Interviews.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// ViewModel for managing collaborators. This partial class contains logic for retrieving and exposing all collaborators for the current session's company.
    /// </summary>
    public partial class CollaboratorsViewModel : ObservableObject
    {
        private readonly ICollaboratorsService collaboratorsService;

        private readonly SessionService sessionService;

        [ObservableProperty]
        private List<Company> allCollaborators = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaboratorsViewModel"/> class.
        /// Collaborators view model constructor that populates the list of all the collaborators.
        /// </summary>
        /// <param name="collaboratorsService">The service used to manage collaborator data.</param>
        /// <param name="sessionService">The service providing information about the current session and logged-in user.</param>
        public CollaboratorsViewModel(ICollaboratorsService collaboratorsService, SessionService sessionService)
        {
            this.collaboratorsService = collaboratorsService;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Loads all collaborators asynchronously.
        /// </summary>
        public async Task LoadCollaboratorsAsync()
        {
            try
            {
                var collaborators = await this.collaboratorsService.GetAllCollaborators(this.sessionService.LoggedInUser.CompanyId);
                this.AllCollaborators = collaborators ?? new List<Company>();
            }
            catch
            {
                // Handle error silently for now
                this.AllCollaborators = new List<Company>();
            }
        }
    }
}

