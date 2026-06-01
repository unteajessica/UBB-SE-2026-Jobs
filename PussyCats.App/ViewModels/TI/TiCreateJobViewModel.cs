using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels.TI;

public partial class TiCreateJobViewModel : DispatchableObservableObject
{
    private readonly ITiJobsService jobsService;
    private readonly SessionContext session;

    [ObservableProperty] private string jobTitle = string.Empty;
    [ObservableProperty] private string industryField = string.Empty;
    [ObservableProperty] private string jobType = string.Empty;
    [ObservableProperty] private string experienceLevel = string.Empty;
    [ObservableProperty] private string jobDescription = string.Empty;
    [ObservableProperty] private string jobLocation = string.Empty;
    [ObservableProperty] private string salaryText = string.Empty;
    [ObservableProperty] private DateTimeOffset? startDate;
    [ObservableProperty] private DateTimeOffset? endDate;
    [ObservableProperty] private DateTimeOffset? deadline;
    [ObservableProperty] private double availablePositions = 1;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private bool savedSuccessfully;
    [ObservableProperty] private string validationError = string.Empty;

    public ObservableCollection<TiSkillPickItem> SkillRows { get; } = new();

    public TiCreateJobViewModel(ITiJobsService jobsService, SessionContext session)
    {
        this.jobsService = jobsService;
        this.session = session;
    }

    public async Task InitializeAsync()
    {
        var skills = await jobsService.GetAllSkillsAsync();
        SkillRows.Clear();
        foreach (var s in skills)
            SkillRows.Add(new TiSkillPickItem { Skill = s });
    }

    [RelayCommand]
    public async Task SaveJobAsync()
    {
        ValidationError = string.Empty;

        if (string.IsNullOrWhiteSpace(JobTitle)) { ValidationError = "Job title is required."; return; }
        if (string.IsNullOrWhiteSpace(JobType)) { ValidationError = "Job type is required."; return; }
        if (AvailablePositions < 1) { ValidationError = "Available positions must be at least 1."; return; }

        int? salary = null;
        if (!string.IsNullOrWhiteSpace(SalaryText))
        {
            if (!int.TryParse(SalaryText, out int s) || s < 0) { ValidationError = "Salary must be a positive number."; return; }
            salary = s;
        }

        IsSaving = true;

        var job = new Job
        {
            CompanyId = session.CompanyId ?? 1,
            JobTitle = JobTitle.Trim(),
            IndustryField = IndustryField.Trim(),
            JobType = JobType,
            ExperienceLevel = ExperienceLevel,
            JobDescription = JobDescription.Trim(),
            JobLocation = JobLocation.Trim(),
            AvailablePositions = (int)AvailablePositions,
            Salary = salary,
            StartDate = StartDate?.DateTime,
            EndDate = EndDate?.DateTime,
            Deadline = Deadline?.DateTime,
            PostedAt = DateTime.UtcNow,
            AmountPayed = 0,
        };

        await jobsService.AddJobAsync(job);
        IsSaving = false;
        SavedSuccessfully = true;
    }
}
