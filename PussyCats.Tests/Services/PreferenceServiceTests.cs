using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class PreferenceServiceTests
{
    private readonly FakeUserRepository repo = new();
    private readonly PreferenceService service;

    public PreferenceServiceTests()
    {
        service = new PreferenceService(repo);
    }

    [Fact]
    public async Task GetByUserIdAsync_UserIsMissing_ReturnsEmptyPreferences()
    {
        var result = await service.GetByUserIdAsync(99);

        result.Roles.Should().BeEmpty();
        result.WorkMode.Should().Be(default);
        result.Location.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_UserExists_TranslatesUserFieldsIntoUserPreferences()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.PreferredEmploymentType = "BackendDeveloper,FrontendDeveloper";
        user.WorkModePreference = "Remote";
        user.LocationPreference = "Cluj-Napoca, Romania";
        repo.Seed(user);

        var result = await service.GetByUserIdAsync(1);

        result.Roles.Should().Equal(JobRole.BackendDeveloper, JobRole.FrontendDeveloper);
        result.WorkMode.Should().Be(WorkMode.Remote);
        result.Location.Should().Be("Cluj-Napoca, Romania");
    }

    [Fact]
    public async Task GetByUserIdAsync_FieldsAreBlank_SkipsBlankFields()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.PreferredEmploymentType = string.Empty;
        user.WorkModePreference = "Remote";
        user.LocationPreference = string.Empty;
        repo.Seed(user);

        var result = await service.GetByUserIdAsync(1);

        result.Roles.Should().BeEmpty();
        result.WorkMode.Should().Be(WorkMode.Remote);
        result.Location.Should().BeEmpty();
    }

    [Fact]
    public async Task SavePreferencesAsync_ValidPreferencesProvided_WritesCombinedRoleString()
    {
        repo.Seed(new UserBuilder().WithId(1).Build());

        await service.SavePreferencesAsync(
            1,
            new[] { JobRole.BackendDeveloper, JobRole.DevOpsEngineer },
            WorkMode.Hybrid,
            "Berlin, Germany");

        var user = await repo.GetByIdAsync(1);
        user!.PreferredEmploymentType.Should().Be("BackendDeveloper,DevOpsEngineer");
        user.WorkModePreference.Should().Be("Hybrid");
        user.LocationPreference.Should().Be("Berlin, Germany");
    }

    [Fact]
    public async Task SavePreferencesAsync_TooFewRolesProvided_ThrowsArgumentException()
    {
        Func<Task> act = () => service.SavePreferencesAsync(
            1,
            Array.Empty<JobRole>(),
            WorkMode.Remote,
            "Anywhere");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SavePreferencesAsync_TooManyRolesProvided_ThrowsArgumentException()
    {
        Func<Task> act = () => service.SavePreferencesAsync(
            1,
            new[]
            {
                JobRole.BackendDeveloper,
                JobRole.FrontendDeveloper,
                JobRole.DevOpsEngineer,
                JobRole.DataAnalyst,
            },
            WorkMode.Remote,
            "Anywhere");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SavePreferencesAsync_UserIsMissing_SilentlyReturns()
    {
        Func<Task> act = () => service.SavePreferencesAsync(
            99,
            new[] { JobRole.BackendDeveloper },
            WorkMode.Remote,
            "Anywhere");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SearchLocationsAsync_QueryIsBlank_ReturnsEmptyList()
    {
        (await service.SearchLocationsAsync("")).Should().BeEmpty();
        (await service.SearchLocationsAsync("   ")).Should().BeEmpty();
    }

    [Fact]
    public async Task SearchLocationsAsync_ValidQueryProvided_MatchesCaseInsensitively()
    {
        var matches = await service.SearchLocationsAsync("cluj");

        matches.Should().NotBeEmpty();
        matches.Should().Contain(loc => loc.Contains("Cluj-Napoca", StringComparison.Ordinal));
    }
}