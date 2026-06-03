using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

/// <summary>
/// Backs the desktop "Skill Tests" tab. Sources the user's real test results from the TI
/// engine (<see cref="ITiTestService"/>) rather than the PussyCats SkillTests table — each
/// card is a TI test plus the user's attempt (status + real percentage score).
/// </summary>
public class TestDashboardViewModel : DispatchableObservableObject
{
    private readonly ITiTestService tiTestService;
    private readonly SessionContext session;

    private List<SkillTestCardViewModel> testCards = new();
    private string? errorMessage;

    public TestDashboardViewModel(ITiTestService tiTestService, SessionContext session)
    {
        this.tiTestService = tiTestService;
        this.session = session;
    }

    public List<SkillTestCardViewModel> TestCards
    {
        get => testCards;
        private set => SetProperty(ref testCards, value);
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadTestsAsync(User? user = null, CancellationToken cancellationToken = default)
    {
        var userId = user?.UserId > 0 ? user.UserId : ViewModelSupport.ResolveUserId(session);

        try
        {
            var tests = await tiTestService.GetAllAsync();

            var cards = new List<SkillTestCardViewModel>();
            foreach (var test in tests)
            {
                // No "all attempts for a user" endpoint on the TI API — fetch the single
                // attempt per test (≈5 tests, acceptable fan-out).
                var attempt = await tiTestService.GetAttemptByUserAndTestAsync(userId, test.Id);
                cards.Add(new SkillTestCardViewModel(test, attempt));
            }

            TestCards = cards;
            ErrorMessage = cards.Count == 0 ? "No skill tests are available yet." : null;
        }
        catch (Exception exception)
        {
            TestCards = new List<SkillTestCardViewModel>();
            ErrorMessage = $"Couldn't load skill tests. Is the Tests service running? ({exception.Message})";
        }
    }
}
