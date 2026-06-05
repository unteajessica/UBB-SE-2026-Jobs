using System.ComponentModel.Design;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.Preferences;

namespace PussyCats.Tests.Services;

public class PreferenceServiceTests
{
    private const int MissingUserId = 99;
    private const int ExistingUserId = 1;

    private const string ExistingRoles = "BackendDeveloper,FrontendDeveloper";
    private const string ExistingLocation = "Cluj-Napoca, Romania";
    private const string ExpectedSearchLocation = "Cluj-Napoca";

    private const string SavedLocation = "Berlin, Germany";
    private const string SavedRoleCsv = "BackendDeveloper,DevOpsEngineer";
    private const string AnyLocation = "Anywhere";
    private const string SearchQuery = "cluj";

    private readonly FakeUserRepository userRepository = new();
    private readonly PreferenceService preferenceService;

    public PreferenceServiceTests()
    {
        preferenceService = new PreferenceService(userRepository);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserIsMissing_ReturnsEmptyPreferences()
    {
        var result = await preferenceService.GetByUserIdAsync(MissingUserId);

        Assert.Empty(result.Roles);
        Assert.Equal(default, result.WorkMode);
        Assert.Empty(result.Location);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserExists_TranslatesUserFieldsIntoUserPreferences()
    {
        var user = new UserBuilder().WithId(ExistingUserId).Build();
        user.PreferredEmploymentType = ExistingRoles;
        user.WorkModePreference = "Remote";
        user.LocationPreference = ExistingLocation;
        userRepository.Seed(user);

        var result = await preferenceService.GetByUserIdAsync(ExistingUserId);

        Assert.Equal(new[] { JobRole.BackendDeveloper, JobRole.FrontendDeveloper }, result.Roles);
        Assert.Equal(WorkMode.Remote, result.WorkMode);
        Assert.Equal(ExistingLocation, result.Location);
    }

    [Fact]
    public async Task GetByUserIdAsync_FieldsAreBlank_SkipsBlankFields()
    {
        var user = new UserBuilder().WithId(ExistingUserId).Build();
        user.PreferredEmploymentType = string.Empty;
        user.WorkModePreference = "Remote";
        user.LocationPreference = string.Empty;
        userRepository.Seed(user);

        var result = await preferenceService.GetByUserIdAsync(ExistingUserId);

        Assert.Empty(result.Roles);
        Assert.Equal(WorkMode.Remote, result.WorkMode);
        Assert.Empty(result.Location);
    }

    [Fact]
    public async Task SavePreferencesAsync_ValidPreferencesProvided_WritesCombinedRoleString()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).Build());

        await preferenceService.SavePreferencesAsync(
            ExistingUserId,
            new[] { JobRole.BackendDeveloper, JobRole.DevOpsEngineer },
            WorkMode.Hybrid,
            SavedLocation);

        var user = await userRepository.GetByIdAsync(ExistingUserId);
        Assert.Equal(SavedRoleCsv, user!.PreferredEmploymentType);
        Assert.Equal("Hybrid", user.WorkModePreference);
        Assert.Equal(SavedLocation, user.LocationPreference);
    }

    [Fact]
    public async Task SavePreferencesAsync_TooFewRolesProvided_ThrowsArgumentException()
    {
        Func<Task> act = () => preferenceService.SavePreferencesAsync(
            ExistingUserId,
            Array.Empty<JobRole>(),
            WorkMode.Remote,
            AnyLocation);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task SavePreferencesAsync_TooManyRolesProvided_ThrowsArgumentException()
    {
        Func<Task> act = () => preferenceService.SavePreferencesAsync(
            ExistingUserId,
            new[]
            {
                JobRole.BackendDeveloper,
                JobRole.FrontendDeveloper,
                JobRole.DevOpsEngineer,
                JobRole.DataAnalyst,
            },
            WorkMode.Remote,
            AnyLocation);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task SavePreferencesAsync_UserIsMissing_SilentlyReturns()
    {
        Func<Task> act = () => preferenceService.SavePreferencesAsync(
            MissingUserId,
            new[] { JobRole.BackendDeveloper },
            WorkMode.Remote,
            AnyLocation);

        var ex = await Record.ExceptionAsync(act);
        Assert.Null(ex);
    }

    [Fact]
    public async Task SearchLocationsAsync_QueryIsBlank_ReturnsEmptyList()
    {
        var searchResultOfLocationQuery = await preferenceService.SearchLocationsAsync("");
        Assert.Empty(searchResultOfLocationQuery);
    }

    [Fact]
    public async Task SearchLocationsAsync_ValidQueryProvided_MatchesCaseInsensitively()
    {
        var searchResultOfLocationQuery = await preferenceService.SearchLocationsAsync(SearchQuery);

        Assert.NotEmpty(searchResultOfLocationQuery);
        Assert.Contains(searchResultOfLocationQuery, location => location.Contains(ExpectedSearchLocation, StringComparison.Ordinal));
    }
}
