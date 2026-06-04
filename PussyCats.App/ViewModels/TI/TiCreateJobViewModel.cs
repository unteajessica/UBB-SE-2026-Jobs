using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Skills;

namespace PussyCats.App.ViewModels.TI;

public partial class TiCreateJobViewModel : DispatchableObservableObject
{
    private readonly IJobService jobService;
    private readonly ISkillService skillService;
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

    public TiCreateJobViewModel(IJobService jobService, ISkillService skillService, SessionContext session)
    {
        this.jobService = jobService;
        this.skillService = skillService;
        this.session = session;
    }

    public async Task InitializeAsync()
    {
        var skills = await skillService.GetAllAsync();
        SkillRows.Clear();
        foreach (var skill in skills)
            SkillRows.Add(new TiSkillPickItem { Skill = TiJobMapper.ToDto(skill) });
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
            if (!int.TryParse(SalaryText, out int salaryNumber) || salaryNumber < 0) { ValidationError = "Salary must be a positive number."; return; }
            salary = salaryNumber;
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

        await jobService.AddAsync(job);
        IsSaving = false;
        SavedSuccessfully = true;
    }
}
