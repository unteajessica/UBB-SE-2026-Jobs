using System.ComponentModel.Design;
using FluentAssertions;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
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

        result.Roles.Should().BeEmpty();
        result.WorkMode.Should().Be(default);
        result.Location.Should().BeEmpty();
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

        result.Roles.Should().Equal(JobRole.BackendDeveloper, JobRole.FrontendDeveloper);
        result.WorkMode.Should().Be(WorkMode.Remote);
        result.Location.Should().Be(ExistingLocation);
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

        result.Roles.Should().BeEmpty();
        result.WorkMode.Should().Be(WorkMode.Remote);
        result.Location.Should().BeEmpty();
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
        user!.PreferredEmploymentType.Should().Be(SavedRoleCsv);
        user.WorkModePreference.Should().Be("Hybrid");
        user.LocationPreference.Should().Be(SavedLocation);
    }

    [Fact]
    public async Task SavePreferencesAsync_TooFewRolesProvided_ThrowsArgumentException()
    {
        Func<Task> act = () => preferenceService.SavePreferencesAsync(
            ExistingUserId,
            Array.Empty<JobRole>(),
            WorkMode.Remote,
            AnyLocation);

        await act.Should().ThrowAsync<ArgumentException>();
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

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SavePreferencesAsync_UserIsMissing_SilentlyReturns()
    {
        Func<Task> act = () => preferenceService.SavePreferencesAsync(
            MissingUserId,
            new[] { JobRole.BackendDeveloper },
            WorkMode.Remote,
            AnyLocation);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SearchLocationsAsync_QueryIsBlank_ReturnsEmptyList()
    {
        var searchResultOfLocationQuery = await preferenceService.SearchLocationsAsync("");
        searchResultOfLocationQuery.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchLocationsAsync_ValidQueryProvided_MatchesCaseInsensitively()
    {
        var searchResultOfLocationQuery = await preferenceService.SearchLocationsAsync(SearchQuery);

        searchResultOfLocationQuery.Should().NotBeEmpty();
        searchResultOfLocationQuery.Should().Contain(location => location.Contains(ExpectedSearchLocation, StringComparison.Ordinal));
    }
}
