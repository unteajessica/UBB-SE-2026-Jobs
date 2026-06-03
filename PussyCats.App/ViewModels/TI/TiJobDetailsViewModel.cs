using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels.TI;

public partial class TiJobDetailsViewModel : DispatchableObservableObject
{
    private readonly ITiApplicantService applicantService;
    private readonly SessionContext session;

    [ObservableProperty] private TiJobPostingDto? currentJob;
    [ObservableProperty] private bool isCompanyMode;
    [ObservableProperty] private bool isBusy;

    public TiJobDetailsViewModel(ITiApplicantService applicantService, SessionContext session)
    {
        this.applicantService = applicantService;
        this.session = session;
    }

    public void Load(TiJobPostingDto job)
    {
        CurrentJob = job;
        IsCompanyMode = session.Mode == AppMode.Company;
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
                return (false, "You have already submitted an application for this job. You cannot apply twice.");

            var applicant = new TiApplicantDto
            {
                JobId = CurrentJob.JobId,
                UserId = session.UserId,
                AppliedAt = DateTime.UtcNow,
                ApplicationStatus = "Pending",
            };

            var result = await applicantService.CreateAsync(applicant);
            return result is null
                ? (false, "Failed to submit application. Please try again.")
                : (true, "Application submitted successfully!");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
