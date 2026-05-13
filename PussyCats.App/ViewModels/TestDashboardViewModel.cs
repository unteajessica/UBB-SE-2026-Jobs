using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats_App.Services.SkillTestService;

namespace PussyCats.App.ViewModels;

public class TestDashboardViewModel : DispatchableObservableObject
{
    private readonly ISkillTestService skillTestService;
    private readonly UserProfileViewModel userProfileViewModel;
    private readonly SessionContext session;

    private List<SkillTestCardViewModel> testCards = new();

    public TestDashboardViewModel(
        ISkillTestService skillTestService,
        UserProfileViewModel userProfileViewModel,
        SessionContext session)
    {
        this.skillTestService = skillTestService;
        this.userProfileViewModel = userProfileViewModel;
        this.session = session;
    }

    public List<SkillTestCardViewModel> TestCards
    {
        get => testCards;
        private set => SetProperty(ref testCards, value);
    }

    public async Task LoadTestsAsync(User? user = null, CancellationToken cancellationToken = default)
    {
        var userId = user?.UserId > 0 ? user.UserId : ViewModelSupport.ResolveUserId(session);
        var tests = await skillTestService.GetTestsForUserAsync(userId, cancellationToken);

        TestCards = tests
            .Select(test => new SkillTestCardViewModel(test, skillTestService, userProfileViewModel))
            .ToList();

        foreach (var card in TestCards)
        {
            await card.LoadCardAsync(cancellationToken);
        }
    }

    public void GoToAllTestsCommand()
    {
    }
}
