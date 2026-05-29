namespace Tests_and_Interviews.ViewModels;

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

/// <summary>
/// Represents a row in the collaborators preview list for a company profile.
/// </summary>
public sealed class CompanyCollabListRow
{
    /// <summary>
    /// Gets or sets the name of the collaborator.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Rows for the "Posted jobs" / "Events" preview lists on the company view profile page.
/// </summary>
public sealed class CompanyProfileListRow
{
    /// <summary>
    /// Gets or sets the title of the job or event.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subtitle (description) of the job or event.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;
}

/// <summary>
/// One trending skill row for the statistics sidebar (frontend display; fill from analytics later).
/// </summary>
public sealed class CompanyTrendingSkillRow
{
    /// <summary>
    /// Gets or sets the rank of the trending skill.
    /// </summary>
    public string Rank { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the skill.
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detail or percentage of the skill.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for the Company Profile view, handling presentation logic and statistical summaries.
/// </summary>
public partial class CompanyProfileViewModel : ObservableObject
{
    private const int MaximumTopJobsCount = 3;
    private const int MaximumTopEventsCount = 3;
    private const int MaximumTopCollaboratorsCount = 7;
    private const int MaximumTrendingSkillsCount = 3;
    private const int MaximumScenarioCount = 2;
    private const int InitialScenarioIndex = 0;
    private const int TotalProfileTasksCount = 5;
    private const int EmptyTaskCount = 0;
    private const int EmptyCompletionPercentage = 0;
    private const int BaseDisplayRankOffset = 1;
    private const int InvalidIndexFallback = 0;

    private const string ProfileLoadErrorMessage = "We could not load this company profile.";
    private const string EmptySkillNameFallback = "—";
    private const string EmptySkillPercentageFallback = "0%";
    private const string FormattedSkillPercentageSuffix = "%";

    private const string DataUriPrefix = "data:image/";
    private const string Base64Marker = ";base64,";
    private const string HintNoLogo = "(no logo)";
    private const string HintLogoSet = "(logo set)";
    private const string HintLogoRenderError = "(logo could not be rendered)";
    private const string HintNoImage = "(no image)";
    private const string HintImageSet = "(image set)";
    private const string HintImageRenderError = "(image could not be rendered)";

    private readonly ICompanyService companyService;
    private readonly IGameService gameService;
    private readonly IEventsService eventsService;
    private readonly IJobsService jobsService;
    private readonly SessionService sessionService;
    private readonly ICollaboratorsService collaboratorsService;
    private readonly IProfileCompletionCalculator calculator;

    private int currentScenarioIndex;
    private ObservableCollection<CompanyProfileListRow> top3JobPreviews = new();
    private ObservableCollection<CompanyProfileListRow> top3EventPreviews = new();
    private ObservableCollection<CompanyCollabListRow> top3CollabsPreviews = new();
    private string buddyImagePath = string.Empty;

    /// <summary>
    /// Gets or sets the action to perform when the profile image is successfully decoded.
    /// </summary>
    public Action<byte[]>? OnProfileImageDecoded { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when the profile image fails to load or is empty.
    /// </summary>
    public Action? OnProfileImageCleared { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when the company logo is successfully decoded.
    /// </summary>
    public Action<byte[]>? OnLogoDecoded { get; set; }

    /// <summary>
    /// Gets or sets the action to perform when the company logo fails to load or is empty.
    /// </summary>
    public Action? OnLogoCleared { get; set; }

    /// <summary>
    /// Gets or sets the hint text for the profile picture.
    /// </summary>
    [ObservableProperty]
    private string profilePictureHintText = string.Empty;

    /// <summary>
    /// Gets or sets the hint text for the company logo.
    /// </summary>
    [ObservableProperty]
    private string companyLogoHintText = string.Empty;

    /// <summary>
    /// Gets or sets the current scenario question being asked.
    /// </summary>
    [ObservableProperty]
    private string currentQuestion = string.Empty;

    /// <summary>
    /// Gets or sets the choices available for the current scenario.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> currentChoices = new ();

    /// <summary>
    /// Gets or sets the feedback provided based on the scenario choice.
    /// </summary>
    [ObservableProperty]
    private string feedback = string.Empty;

    /// <summary>
    /// Gets the image path for the buddy based on the current game buddy ID.
    /// </summary>
    public string BuddyImagePath => this.buddyImagePath;

    /// <summary>
    /// Gets or sets the welcome message from the buddy interacting in the game.
    /// </summary>
    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    /// <summary>
    /// Gets or sets the current game state for the scenario interactions.
    /// </summary>
    [ObservableProperty]
    private GameState currentState = GameState.NotCompleted;

    /// <summary>
    /// Gets or sets the current company being viewed.
    /// </summary>
    [ObservableProperty]
    private Company? company;

    /// <summary>
    /// Gets or sets the message explaining load state or errors.
    /// </summary>
    [ObservableProperty]
    private string loadMessage = string.Empty;

    /// <summary>
    /// Gets or sets the profile completion percentage.
    /// </summary>
    [ObservableProperty]
    private int completionPercentage;

    /// <summary>
    /// Gets or sets the count of completed profile tasks.
    /// </summary>
    [ObservableProperty]
    private int completedTasksCount;

    /// <summary>
    /// Gets or sets the remaining tasks necessary to complete the profile.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> remainingTasks = new ();

    /// <summary>
    /// Gets or sets the summary details of applicants.
    /// </summary>
    [ObservableProperty]
    private string applicantSummary = string.Empty;

    /// <summary>
    /// Gets or sets the collection of trending skills calculated for the company.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CompanyTrendingSkillRow> trendingSkills = new ();

    /// <summary>
    /// Gets the top 3 previews of jobs posted by the company.
    /// </summary>
    public ObservableCollection<CompanyProfileListRow> Top3JobPreviews => this.top3JobPreviews;

    /// <summary>
    /// Gets the top 3 previews of upcoming events for the company.
    /// </summary>
    public ObservableCollection<CompanyProfileListRow> Top3EventPreviews => this.top3EventPreviews;

    /// <summary>
    /// Gets the top 3 previews of collaborators associated with the company.
    /// </summary>
    public ObservableCollection<CompanyCollabListRow> Top3CollabsPreviews => this.top3CollabsPreviews;

    /// <summary>
    /// Event triggered when navigation to all collaborators is requested.
    /// </summary>
    public event EventHandler? NavigateAllCollaboratorRequested;

    /// <summary>
    /// Event triggered when navigation to profile editing is requested.
    /// </summary>
    public event EventHandler? NavigateEditProfileRequested;

    /// <summary>
    /// Event triggered when navigation to view all events is requested.
    /// </summary>
    public event EventHandler? NavigateAllEventsRequested;

    /// <summary>
    /// Event triggered when navigation to view all jobs is requested.
    /// </summary>
    public event EventHandler? NavigateAllJobsRequested;

    /// <summary>
    /// Gets the ID of the company currently loaded in the profile.
    /// </summary>
    public int CompanyId { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyProfileViewModel"/> class.
    /// </summary>
    /// <param name="companyService">The company service.</param>
    /// <param name="calculator">The profile completion calculator.</param>
    /// <param name="gameService">The game service.</param>
    /// <param name="eventService">The events service.</param>
    /// <param name="sessionService">The current session service.</param>
    /// <param name="collaboratorsService">The collaborators service.</param>
    /// <param name="jobsService">The jobs repository.</param>
    public CompanyProfileViewModel(
        ICompanyService companyService,
        IProfileCompletionCalculator calculator,
        IGameService gameService,
        IEventsService eventService,
        SessionService sessionService,
        ICollaboratorsService collaboratorsService,
        IJobsService jobsService)
    {
        this.gameService = gameService;
        this.companyService = companyService;
        this.calculator = calculator;
        this.eventsService = eventService;
        this.sessionService = sessionService;
        this.collaboratorsService = collaboratorsService;
        this.jobsService = jobsService;
    }

    /// <summary>
    /// Loads the data associated with the specified company ID.
    /// </summary>
    /// <param name="companyId">The ID of the company to load.</param>
    public async Task Load(int companyId)
    {
        this.CompanyId = companyId;
        this.Company = await this.companyService.GetCompanyById(companyId);
        if (this.Company is null)
        {
            this.LoadMessage = ProfileLoadErrorMessage;
            this.CompletionPercentage = EmptyCompletionPercentage;
            this.CompletedTasksCount = EmptyTaskCount;
            this.RemainingTasks.Clear();
            return;
        }

        this.ApplicantSummary = await this.calculator.ApplicantsMessage(companyId);

        this.LoadMessage = string.Empty;
        this.RefreshProfileStatistics();
        await this.FillPreviewSectionsAsync();
        this.ProcessImages();
        await this.GamePreview();
    }

    private void ProcessImages()
    {
        var rawLogo = this.Company?.CompanyLogoPath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawLogo))
        {
            this.CompanyLogoHintText = HintNoLogo;
            this.OnLogoCleared?.Invoke();
        }
        else if (rawLogo.StartsWith(DataUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var base64Index = rawLogo.IndexOf(Base64Marker, StringComparison.OrdinalIgnoreCase);
            if (base64Index >= InvalidIndexFallback)
            {
                var base64 = rawLogo.Substring(base64Index + Base64Marker.Length);
                try
                {
                    var bytes = Convert.FromBase64String(base64);
                    this.CompanyLogoHintText = string.Empty;
                    this.OnLogoDecoded?.Invoke(bytes);
                }
                catch
                {
                    this.CompanyLogoHintText = HintLogoRenderError;
                    this.OnLogoCleared?.Invoke();
                }
            }
            else
            {
                this.CompanyLogoHintText = HintLogoRenderError;
                this.OnLogoCleared?.Invoke();
            }
        }
        else
        {
            this.CompanyLogoHintText = HintLogoSet;
            this.OnLogoCleared?.Invoke();
        }

        var rawPic = this.Company?.ProfilePicturePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawPic))
        {
            this.ProfilePictureHintText = HintNoImage;
            this.OnProfileImageCleared?.Invoke();
        }
        else if (rawPic.StartsWith(DataUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var base64Index = rawPic.IndexOf(Base64Marker, StringComparison.OrdinalIgnoreCase);
            if (base64Index >= InvalidIndexFallback)
            {
                var base64 = rawPic.Substring(base64Index + Base64Marker.Length);
                try
                {
                    var bytes = Convert.FromBase64String(base64);
                    this.ProfilePictureHintText = string.Empty;
                    this.OnProfileImageDecoded?.Invoke(bytes);
                }
                catch
                {
                    this.ProfilePictureHintText = HintImageRenderError;
                    this.OnProfileImageCleared?.Invoke();
                }
            }
            else
            {
                this.ProfilePictureHintText = HintImageRenderError;
                this.OnProfileImageCleared?.Invoke();
            }
        }
        else
        {
            this.ProfilePictureHintText = HintImageSet;
            this.OnProfileImageCleared?.Invoke();
        }
    }

    /// <summary>
    /// Refreshes the profile statistics and updates completion values.
    /// </summary>
    private void RefreshProfileStatistics()
    {
        if (this.Company is null)
        {
            return;
        }

        var (percentage, tasks) = this.calculator.Calculate(this.Company);
        this.CompletionPercentage = percentage;
        this.CompletedTasksCount = TotalProfileTasksCount - tasks.Count;
        if (this.CompletedTasksCount < EmptyTaskCount)
        {
            this.CompletedTasksCount = EmptyTaskCount;
        }

        this.RemainingTasks.Clear();
        foreach (var task in tasks)
        {
            this.RemainingTasks.Add(task);
        }
    }

    private async Task FillPreviewSectionsAsync()
    {
        if (this.Company is null)
        {
            return;
        }

        this.TrendingSkills.Clear();
        var (skillNames, percents) = await this.calculator.GetSkillsTop3Async(this.Company.CompanyId);

        for (int index = 0; index < MaximumTrendingSkillsCount; index++)
        {
            string skillName = index < skillNames.Count ? skillNames[index] : EmptySkillNameFallback;
            string percent = index < percents.Count ? $"{percents[index]}{FormattedSkillPercentageSuffix}" : EmptySkillPercentageFallback;

            this.TrendingSkills.Add(new CompanyTrendingSkillRow
            {
                Rank = (index + BaseDisplayRankOffset).ToString(),
                SkillName = skillName,
                Detail = percent,
            });
        }

        // Load jobs asynchronously
        try
        {
            var jobs = await this.jobsService.GetAllJobsAsync();
            this.top3JobPreviews.Clear();
            foreach (var job in jobs.Take(MaximumTopJobsCount))
            {
                this.top3JobPreviews.Add(new CompanyProfileListRow
                {
                    Title = job.JobTitle,
                    Subtitle = job.JobDescription,
                });
            }
            this.OnPropertyChanged(nameof(this.Top3JobPreviews));
        }
        catch
        {
            // Handle error silently
        }

        // Load events asynchronously
        try
        {
            var events = await this.eventsService.GetCurrentEvents(this.sessionService.LoggedInUser.CompanyId);
            this.top3EventPreviews.Clear();
            foreach (var eventItem in events.Take(MaximumTopEventsCount))
            {
                this.top3EventPreviews.Add(new CompanyProfileListRow
                {
                    Title = eventItem.Title,
                    Subtitle = eventItem.Description,
                });
            }
            this.OnPropertyChanged(nameof(this.Top3EventPreviews));
        }
        catch
        {
            // Handle error silently
        }

        // Load collaborators asynchronously
        try
        {
            var collaborators = await this.collaboratorsService.GetAllCollaborators(this.sessionService.LoggedInUser.CompanyId);
            this.top3CollabsPreviews.Clear();
            foreach (var collaborator in collaborators.Take(MaximumTopCollaboratorsCount))
            {
                this.top3CollabsPreviews.Add(new CompanyCollabListRow
                {
                    Name = collaborator.Name,
                });
            }
            this.OnPropertyChanged(nameof(this.Top3CollabsPreviews));
        }
        catch
        {
            // Handle error silently
        }
    }

    [RelayCommand]
    private void SeeAllCollaborators()
    {
        this.NavigateAllCollaboratorRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditProfile()
    {
        this.NavigateEditProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SeeAllEvents()
    {
        this.NavigateAllEventsRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SeeAllJobs()
    {
        this.NavigateAllJobsRequested?.Invoke(this, EventArgs.Empty);
    }

    partial void OnCurrentStateChanged(GameState value)
    {
        this.OnPropertyChanged(nameof(this.IncompleteGame));
        this.OnPropertyChanged(nameof(this.IsStartVisible));
        this.OnPropertyChanged(nameof(this.IsChoice1Visible));
        this.OnPropertyChanged(nameof(this.IsReaction1Visible));
        this.OnPropertyChanged(nameof(this.IsChoice2Visible));
        this.OnPropertyChanged(nameof(this.IsReaction2Visible));
        this.OnPropertyChanged(nameof(this.IsConclusionVisible));
        this.OnPropertyChanged(nameof(this.IsChoiceActive));
        this.OnPropertyChanged(nameof(this.IsReactionActive));
    }

    /// <summary>
    /// Gets a value indicating whether the game is currently incomplete.
    /// </summary>
    public bool IncompleteGame => this.CurrentState == GameState.NotCompleted;

    /// <summary>
    /// Gets a value indicating whether the start state is visible.
    /// </summary>
    public bool IsStartVisible => this.CurrentState == GameState.Start;

    /// <summary>
    /// Gets a value indicating whether the first choice set is visible.
    /// </summary>
    public bool IsChoice1Visible => this.CurrentState == GameState.Choices1;

    /// <summary>
    /// Gets a value indicating whether the first reaction is visible.
    /// </summary>
    public bool IsReaction1Visible => this.CurrentState == GameState.Reaction1;

    /// <summary>
    /// Gets a value indicating whether the second choice set is visible.
    /// </summary>
    public bool IsChoice2Visible => this.CurrentState == GameState.Choices2;

    /// <summary>
    /// Gets a value indicating whether the second reaction is visible.
    /// </summary>
    public bool IsReaction2Visible => this.CurrentState == GameState.Reaction2;

    /// <summary>
    /// Gets a value indicating whether the conclusion of the game is visible.
    /// </summary>
    public bool IsConclusionVisible => this.CurrentState == GameState.Conclusion;

    /// <summary>
    /// Gets a value indicating whether any choice state is currently active.
    /// </summary>
    public bool IsChoiceActive => this.IsChoice1Visible || this.IsChoice2Visible;

    /// <summary>
    /// Gets a value indicating whether any reaction state is currently active.
    /// </summary>
    public bool IsReactionActive => this.IsReaction1Visible || this.IsReaction2Visible;

    private async Task UpdateScenario()
    {
        if (this.currentScenarioIndex < MaximumScenarioCount)
        {
            this.CurrentQuestion = await this.gameService.ShowScenarioText(this.currentScenarioIndex);

            this.CurrentChoices.Clear();
            var choices = await this.gameService.ShowChoices(this.currentScenarioIndex);
            foreach (var choice in choices)
            {
                this.CurrentChoices.Add(choice);
            }
        }
    }

    /// <summary>
    /// Initializes and previews the game states.
    /// </summary>
    private async Task GamePreview()
    {
        if (await this.gameService.IsPublished())
        {
            // Load buddy image asynchronously
            try
            {
                var buddyId = await this.gameService.GetBuddyId();
                this.buddyImagePath = BuddyImageProvider.GetImagePathById(buddyId);
                this.OnPropertyChanged(nameof(this.BuddyImagePath));
            }
            catch
            {
                // Handle error silently
            }

            this.WelcomeMessage = await this.gameService.ShowCoworker();
            this.CurrentState = GameState.Start;
            this.currentScenarioIndex = InitialScenarioIndex;
            await this.UpdateScenario();
        }
    }

    [RelayCommand]
    private async Task RetryGame()
    {
        await this.GamePreview();
    }

    [RelayCommand]
    private void StartGame()
    {
        this.CurrentState = GameState.Choices1;
    }

    [RelayCommand]
    private async Task SelectChoice(string? choiceText)
    {
        if (string.IsNullOrEmpty(choiceText) || this.CurrentChoices == null)
        {
            return;
        }

        int adviceIndex = this.CurrentChoices.IndexOf(choiceText);
        if (adviceIndex < 0)
        {
            return;
        }

        this.Feedback = await this.gameService.ChoiceMade(this.currentScenarioIndex, adviceIndex);
        this.CurrentState = this.currentScenarioIndex == InitialScenarioIndex ? GameState.Reaction1 : GameState.Reaction2;
    }

    [RelayCommand]
    private async Task GoToNextStep()
    {
        this.currentScenarioIndex++;

        if (this.currentScenarioIndex < MaximumScenarioCount)
        {
            await this.UpdateScenario();
            this.CurrentState = GameState.Choices2;
        }
        else
        {
            this.Feedback = await this.gameService.ShowConclusion();
            this.CurrentState = GameState.Conclusion;
        }
    }
}