using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiJobsViewModel : DispatchableObservableObject
{
    private readonly ITiJobsService jobsService;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<TiJobPostingDto> Jobs { get; } = new();

    public TiJobsViewModel(ITiJobsService jobsService)
    {
        this.jobsService = jobsService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var jobs = await jobsService.GetAllJobsAsync();
        Jobs.Clear();
        foreach (var j in jobs) Jobs.Add(j);
        IsLoading = false;
    }

    public async Task DeleteJobAsync(int jobId)
    {
        await jobsService.DeleteJobAsync(jobId);
        await LoadAsync();
    }
}
