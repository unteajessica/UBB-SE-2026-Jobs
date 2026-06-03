using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiAlreadySubmittedViewModel : DispatchableObservableObject
{
    private readonly ITiTestService testService;
    private readonly SessionContext session;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private int attemptId;
    [ObservableProperty] private bool hasAttempt;

    public TiAlreadySubmittedViewModel(ITiTestService testService, SessionContext session)
    {
        this.testService = testService;
        this.session = session;
    }

    public async Task LoadAsync(int testId)
    {
        IsLoading = true;
        var attempt = await testService.GetAttemptByUserAndTestAsync(session.UserId, testId);
        if (attempt != null)
        {
            AttemptId = attempt.Id;
            HasAttempt = true;
        }
        IsLoading = false;
    }
}
