namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Validators;
    using Tests_and_Interviews.ViewModels;

    /// <summary>
    /// Represents a service for picking images.
    /// </summary>
    public interface IImagePickerService
    {
        /// <summary>
        /// Asynchronously picks an image and returns its file name and byte array.
        /// </summary>
        /// <returns>A task containing the file name and bytes, or null if cancelled.</returns>
        Task<(string FileName, byte[] Bytes)?> PickImageAsync();
    }

    /// <summary>
    /// View model for editing a company profile.
    /// </summary>
    public partial class EditCompanyProfileViewModel : ObservableObject
    {
        private const string MessageCompanyNotFound = "Company not found.";
        private const string DefaultPhotoFileName = "No image selected";
        private const string DataUriPrefix = "data:image/";
        private const string DataUriBase64Infix = ";base64,";
        private const string ExtensionJpg = "jpg";
        private const string MimeTypeJpeg = "jpeg";
        private const int DefaultCountValue = 0;

        private readonly ICompanyService companyService;
        private readonly IGameService gameService;
        private readonly ICompanyValidator companyValidator;
        private readonly IGameValidator gameValidator;
        private readonly IImagePickerService imagePickerService;

        /// <summary>
        /// Gets or sets the action to invoke when a profile preview is requested.
        /// </summary>
        public Action<byte[]>? OnProfilePreviewRequested { get; set; }

        /// <summary>
        /// Gets or sets the action to invoke when a logo preview is requested.
        /// </summary>
        public Action<byte[]>? OnLogoPreviewRequested { get; set; }

        [ObservableProperty]
        private int companyId;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string aboutUs = string.Empty;

        [ObservableProperty]
        private string profilePicturePath = string.Empty;

        [ObservableProperty]
        private string photoFileName = DefaultPhotoFileName;

        [ObservableProperty]
        private string companyLogoPath = string.Empty;

        [ObservableProperty]
        private string logoFileName = DefaultPhotoFileName;

        [ObservableProperty]
        private string location = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        /// <summary>
        /// Gets the game view model for editing game details.
        /// </summary>
        public EditGame EditGame { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditCompanyProfileViewModel"/> class.
        /// </summary>
        /// <param name="companyService">The company service.</param>
        /// <param name="gameService">The game service.</param>
        /// <param name="companyValidator">The company validator.</param>
        /// <param name="gameValidator">The game validator.</param>
        /// <param name="imagePickerService">The image picker service.</param>
        public EditCompanyProfileViewModel(
            ICompanyService companyService,
            IGameService gameService,
            ICompanyValidator companyValidator,
            IGameValidator gameValidator,
            IImagePickerService imagePickerService)
        {
            this.companyService = companyService;
            this.gameService = gameService;
            this.companyValidator = companyValidator;
            this.gameValidator = gameValidator;
            this.imagePickerService = imagePickerService;

            EditGame = new EditGame(this.gameService, this.gameValidator);
        }

        [RelayCommand]
        private async Task PickProfileImageAsync()
        {
            var result = await imagePickerService.PickImageAsync();
            if (result == null)
            {
                return;
            }

            PhotoFileName = result.Value.FileName;
            var extension = System.IO.Path.GetExtension(result.Value.FileName).TrimStart('.').ToLowerInvariant();
            var mimeSubtype = extension == ExtensionJpg ? MimeTypeJpeg : extension;

            ProfilePicturePath = $"{DataUriPrefix}{mimeSubtype}{DataUriBase64Infix}{Convert.ToBase64String(result.Value.Bytes)}";
            OnProfilePreviewRequested?.Invoke(result.Value.Bytes);
        }

        [RelayCommand]
        private async Task PickLogoImageAsync()
        {
            var result = await imagePickerService.PickImageAsync();
            if (result == null)
            {
                return;
            }

            LogoFileName = result.Value.FileName;
            var extension = System.IO.Path.GetExtension(result.Value.FileName).TrimStart('.').ToLowerInvariant();
            var mimeSubtype = extension == ExtensionJpg ? MimeTypeJpeg : extension;

            CompanyLogoPath = $"{DataUriPrefix}{mimeSubtype}{DataUriBase64Infix}{Convert.ToBase64String(result.Value.Bytes)}";
            OnLogoPreviewRequested?.Invoke(result.Value.Bytes);
        }

        /// <summary>
        /// Loads the company details by the given company ID.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        public async Task Load(int companyId)
        {
            this.CompanyId = companyId;
            this.StatusMessage = string.Empty;

            Company? existingCompany = await this.companyService.GetCompanyById(companyId);
            if (existingCompany is null)
            {
                this.StatusMessage = MessageCompanyNotFound;
                return;
            }

            this.Name = existingCompany.Name;
            this.CompanyLogoPath = existingCompany.CompanyLogoPath ?? string.Empty;
            this.AboutUs = existingCompany.AboutUs ?? string.Empty;
            this.ProfilePicturePath = existingCompany.ProfilePicturePath ?? string.Empty;
            this.Location = existingCompany.Location ?? string.Empty;
            this.Email = existingCompany.Email ?? string.Empty;

            // Set file names to display in UI - these are just display values
            if (!string.IsNullOrWhiteSpace(existingCompany.ProfilePicturePath))
            {
                this.PhotoFileName = "(profile picture set)";
            }
            else
            {
                this.PhotoFileName = DefaultPhotoFileName;
            }

            if (!string.IsNullOrWhiteSpace(existingCompany.CompanyLogoPath))
            {
                this.LogoFileName = "(logo set)";
            }
            else
            {
                this.LogoFileName = DefaultPhotoFileName;
            }

            await this.EditGame.LoadStoredGame();
        }

        private Company ToCompany(int postedJobsCount, int collaboratorsCount, Company? existingCompany = null)
        {
            var company = new Company(
                name: this.Name,
                aboutUs: this.AboutUs,
                pfpUrl: this.ProfilePicturePath,
                logoUrl: this.CompanyLogoPath,
                location: this.Location,
                email: this.Email,
                companyId: this.CompanyId,
                postedJobsCount: postedJobsCount,
                collaboratorsCount: collaboratorsCount);

            // Preserve all other properties from the existing company to avoid data loss
            if (existingCompany != null)
            {
                company.BuddyName = existingCompany.BuddyName;
                company.AvatarId = existingCompany.AvatarId;
                company.FinalQuote = existingCompany.FinalQuote;
                company.BuddyDescription = existingCompany.BuddyDescription;
                company.Scen1Text = existingCompany.Scen1Text;
                company.Scen1Answer1 = existingCompany.Scen1Answer1;
                company.Scen1Answer2 = existingCompany.Scen1Answer2;
                company.Scen1Answer3 = existingCompany.Scen1Answer3;
                company.Scen1Reaction1 = existingCompany.Scen1Reaction1;
                company.Scen1Reaction2 = existingCompany.Scen1Reaction2;
                company.Scen1Reaction3 = existingCompany.Scen1Reaction3;
                company.Scen2Text = existingCompany.Scen2Text;
                company.Scen2Answer1 = existingCompany.Scen2Answer1;
                company.Scen2Answer2 = existingCompany.Scen2Answer2;
                company.Scen2Answer3 = existingCompany.Scen2Answer3;
                company.Scen2Reaction1 = existingCompany.Scen2Reaction1;
                company.Scen2Reaction2 = existingCompany.Scen2Reaction2;
                company.Scen2Reaction3 = existingCompany.Scen2Reaction3;
            }

            return company;
        }

        /// <summary>
        /// Tries to save the edited company profile and game data.
        /// </summary>
        /// <returns>An error message string if the save fails; otherwise, null.</returns>
        public async Task<string?> TrySave()
        {
            StatusMessage = string.Empty;

            Company? existingCompany = await companyService.GetCompanyById(CompanyId);
            int existingPostedJobsCount = existingCompany?.PostedJobsCount ?? DefaultCountValue;
            int existingCollaboratorsCount = existingCompany?.CollaboratorsCount ?? DefaultCountValue;
            ICollection<Collaborator> collaboratorsCopy = existingCompany?.Collaborators ?? new List<Collaborator>();

            try
            {
                companyValidator.ValidateName(Name);

                var scenarioTuplesList = EditGame.Scenarios
                    .Select(scenario => (
                        scenarioText: scenario.ScenarioText ?? string.Empty,
                        choices: (IReadOnlyList<(string advice, string feedback)>)scenario.Choices
                            .Select(choice => (
                                advice: choice.Advice ?? string.Empty,
                                feedback: choice.Feedback ?? string.Empty))
                            .ToList()))
                    .ToList();

                gameValidator.ValidateForActivation(scenarioTuplesList, EditGame.Conclusion ?? string.Empty);

                Game newGame = gameService.CreateGameFromInput(
                    buddyId: EditGame.SelectedBuddyId,
                    buddyName: EditGame.BuddyName,
                    buddyIntroduction: EditGame.BuddyIntroduction,
                    scenarios: scenarioTuplesList,
                    conclusion: EditGame.Conclusion ?? string.Empty,
                    publish: true);

                Company updatedCompany = ToCompany(existingPostedJobsCount, existingCollaboratorsCount, existingCompany);
                updatedCompany.Collaborators = collaboratorsCopy;
                updatedCompany.Game = newGame;

                companyService.UpdateCompany(updatedCompany);
                gameService.Save(newGame);

                return null;
            }
            catch (Exception exception)
            {
                StatusMessage = exception.Message;
                return exception.Message;
            }
        }
    }

    /// <summary>
    /// View model for editing a game.
    /// </summary>
    public partial class EditGame : ObservableObject
    {
        private const int RequiredScenariosCount = 2;
        private const int ChoicesPerScenarioCount = 3;
        private const int DefaultBuddyId = 1;
        private const string MessageGameCreatedSuccessfully = "Game created and saved successfully.";
        private const string MessageGameCreateFailedPrefix = "Failed to create game: ";

        private readonly IGameService gameService;
        private readonly IGameValidator gameValidator;

        /// <summary>
        /// Gets the collection of scenario inputs.
        /// </summary>
        public ObservableCollection<ScenarioInput> Scenarios { get; } = new ObservableCollection<ScenarioInput>();

        /// <summary>
        /// Gets the collection of available buddy IDs.
        /// </summary>
        public ObservableCollection<int> AvailableBuddyIds { get; } = new ObservableCollection<int> { 0, 1 };

        [ObservableProperty]
        private int selectedBuddyId = DefaultBuddyId;

        /// <summary>
        /// Gets the image path associated with the currently selected buddy.
        /// </summary>
        public string BuddyImagePath => BuddyImageProvider.GetImagePathById(this.SelectedBuddyId);

        partial void OnSelectedBuddyIdChanged(int value)
        {
            this.OnPropertyChanged(nameof(this.BuddyImagePath));
        }

        [ObservableProperty]
        private string buddyName = string.Empty;

        [ObservableProperty]
        private string buddyIntroduction = string.Empty;

        [ObservableProperty]
        private string conclusion = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditGame"/> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        /// <param name="gameValidator">The game validator.</param>
        public EditGame(IGameService gameService, IGameValidator gameValidator)
        {
            this.gameService = gameService;
            this.gameValidator = gameValidator;

            for (int scenarioIndex = 0; scenarioIndex < RequiredScenariosCount; scenarioIndex++)
            {
                var scenarioInput = new ScenarioInput();

                for (int choiceIndex = 0; choiceIndex < ChoicesPerScenarioCount; choiceIndex++)
                {
                    scenarioInput.Choices.Add(new AdviceChoiceInput());
                }

                this.Scenarios.Add(scenarioInput);
            }

            this.StatusMessage = string.Empty;
        }

        public async Task LoadStoredGame()
        {
            this.ApplyLoadedGame(await this.gameService.GetStoredGame());
        }

        private void ApplyLoadedGame(Game game)
        {
            if (game == null)
            {
                return;
            }

            this.SelectedBuddyId = game.Buddy.Id;
            this.BuddyName = game.Buddy.Name ?? string.Empty;
            this.BuddyIntroduction = game.Buddy.Introduction ?? string.Empty;
            this.Conclusion = game.Conclusion ?? string.Empty;

            for (int scenarioIndex = 0; scenarioIndex < this.Scenarios.Count && scenarioIndex < game.Scenarios.Count; scenarioIndex++)
            {
                var scenarioViewModel = this.Scenarios[scenarioIndex];
                var scenarioModel = game.Scenarios[scenarioIndex];
                scenarioViewModel.ScenarioText = scenarioModel.Description ?? string.Empty;

                var adviceChoicesList = scenarioModel.AdviceChoices;
                for (int choiceIndex = 0; choiceIndex < scenarioViewModel.Choices.Count && choiceIndex < adviceChoicesList.Count; choiceIndex++)
                {
                    scenarioViewModel.Choices[choiceIndex].Advice = adviceChoicesList[choiceIndex].Advice ?? string.Empty;
                    scenarioViewModel.Choices[choiceIndex].Feedback = adviceChoicesList[choiceIndex].Feedback ?? string.Empty;
                }
            }
        }

        [RelayCommand]
        private void CreateGame()
        {
            try
            {
                var scenarioTuplesList = this.Scenarios
                    .Select(scenario => (
                        scenarioText: scenario.ScenarioText ?? string.Empty,
                        choices: (IReadOnlyList<(string advice, string feedback)>)scenario.Choices
                            .Select(choice => (
                                advice: choice.Advice ?? string.Empty,
                                feedback: choice.Feedback ?? string.Empty))
                            .ToList()))
                    .ToList();

                this.gameValidator.ValidateForActivation(scenarioTuplesList, this.Conclusion ?? string.Empty);

                var newGame = this.gameService.CreateGameFromInput(
                    buddyId: this.SelectedBuddyId,
                    buddyName: this.BuddyName,
                    buddyIntroduction: this.BuddyIntroduction,
                    scenarios: scenarioTuplesList,
                    conclusion: this.Conclusion ?? string.Empty,
                    publish: false);

                this.gameService.Save(newGame);
                this.StatusMessage = MessageGameCreatedSuccessfully;
            }
            catch (Exception exception)
            {
                this.StatusMessage = $"{MessageGameCreateFailedPrefix}{exception.Message}";
            }
        }
    }

    /// <summary>
    /// Represents the input data for a single scenario.
    /// </summary>
    public partial class ScenarioInput : ObservableObject
    {
        [ObservableProperty]
        private string scenarioText = string.Empty;

        /// <summary>
        /// Gets the collection of advice choices available for this scenario.
        /// </summary>
        public ObservableCollection<AdviceChoiceInput> Choices { get; } = new ObservableCollection<AdviceChoiceInput>();
    }

    /// <summary>
    /// Represents the input data for a single advice choice.
    /// </summary>
    public partial class AdviceChoiceInput : ObservableObject
    {
        [ObservableProperty]
        private string advice = string.Empty;

        [ObservableProperty]
        private string feedback = string.Empty;
    }
}