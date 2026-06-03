using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Services.Jobs;

namespace PussyCats.App.ViewModels.TI;

public partial class TiJobsViewModel : DispatchableObservableObject
{
    private readonly IJobService jobService;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<TiJobPostingDto> Jobs { get; } = new();

    public TiJobsViewModel(IJobService jobService)
    {
        this.jobService = jobService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var jobs = await jobService.GetAllAsync();
        Jobs.Clear();
        foreach (var job in jobs) Jobs.Add(TiJobMapper.ToDto(job));
        IsLoading = false;
    }

    public async Task DeleteJobAsync(int jobId)
    {
        await jobService.RemoveAsync(jobId);
        await LoadAsync();
    }
}
