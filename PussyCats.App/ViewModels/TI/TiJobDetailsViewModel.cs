using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Jobs;

namespace PussyCats.App.ViewModels.TI;

public partial class TiJobDetailsViewModel : DispatchableObservableObject
{
    private readonly ITiApplicantService applicantService;
    private readonly IJobService jobService;
    private readonly SessionContext session;

    [ObservableProperty] private TiJobPostingDto? currentJob;
    [ObservableProperty] private bool isCompanyMode;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool hasApplied;

    public TiJobDetailsViewModel(ITiApplicantService applicantService, IJobService jobService, SessionContext session)
    {
        this.applicantService = applicantService;
        this.jobService = jobService;
        this.session = session;
    }

    public async Task LoadAsync(TiJobPostingDto job)
    {
        IsCompanyMode = session.Mode == AppMode.Company;

        // The Jobs listing (IJobService.GetAllAsync) omits required skills; fetch the full job by
        // id so the details view can render them. Fall back to the passed-in object if not found.
        var full = await jobService.GetByIdAsync(job.JobId);
        CurrentJob = full is null ? job : TiJobMapper.ToDto(full);
    }

    public async Task RefreshHasAppliedAsync()
    {
        if (CurrentJob is null || IsCompanyMode || session.UserId <= 0)
        {
            HasApplied = false;
            return;
        }

        HasApplied = await applicantService.HasUserAppliedAsync(CurrentJob.JobId, session.UserId);
    }

    public async Task<(bool Ok, string Message)> ApplyAsync()
    {
        if (CurrentJob is null)
            return (false, "No job selected.");

        if (session.UserId <= 0)
            return (false, "You must be signed in to apply.");

        IsBusy = true;
        try
        {
            if (await applicantService.HasUserAppliedAsync(CurrentJob.JobId, session.UserId))
            {
                HasApplied = true;
                return (false, "You have already submitted an application for this job. You cannot apply twice.");
            }

            var applicant = new TiApplicantDto
            {
                JobId = CurrentJob.JobId,
                UserId = session.UserId,
                AppliedAt = DateTime.UtcNow,
                ApplicationStatus = "Pending",
            };

            var result = await applicantService.CreateAsync(applicant);
            if (result is null)
                return (false, "Failed to submit application. Please try again.");

            HasApplied = true;
            return (true, "Application submitted successfully!");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
