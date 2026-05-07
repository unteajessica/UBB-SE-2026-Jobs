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
    public async Task GetByUserIdAsync_returns_empty_for_missing_user()
    {
        var result = await service.GetByUserIdAsync(99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_translates_user_fields_into_preferences()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.PreferredEmploymentType = "BackendDeveloper,FrontendDeveloper";
        user.WorkModePreference = "Remote";
        user.LocationPreference = "Cluj-Napoca, Romania";
        repo.Seed(user);

        var result = await service.GetByUserIdAsync(1);

        result.Should().HaveCount(4);
        result.Should().Contain(p => p.PreferenceType == "JobRole" && p.Value == "BackendDeveloper");
        result.Should().Contain(p => p.PreferenceType == "JobRole" && p.Value == "FrontendDeveloper");
        result.Should().Contain(p => p.PreferenceType == "WorkMode" && p.Value == "Remote");
        result.Should().Contain(p => p.PreferenceType == "Location" && p.Value == "Cluj-Napoca, Romania");
    }

    [Fact]
    public async Task GetByUserIdAsync_skips_blank_fields()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.PreferredEmploymentType = string.Empty;
        user.WorkModePreference = "Remote";
        user.LocationPreference = string.Empty;
        repo.Seed(user);

        var result = await service.GetByUserIdAsync(1);

        result.Should().HaveCount(1);
        result[0].PreferenceType.Should().Be("WorkMode");
    }

    [Fact]
    public async Task SavePreferencesAsync_writes_combined_role_string()
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
    public async Task SavePreferencesAsync_throws_when_too_few_roles()
    {
        Func<Task> act = () => service.SavePreferencesAsync(
            1,
            Array.Empty<JobRole>(),
            WorkMode.Remote,
            "Anywhere");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SavePreferencesAsync_throws_when_too_many_roles()
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
    public async Task SavePreferencesAsync_silently_returns_when_user_missing()
    {
        Func<Task> act = () => service.SavePreferencesAsync(
            99,
            new[] { JobRole.BackendDeveloper },
            WorkMode.Remote,
            "Anywhere");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SearchLocationsAsync_returns_empty_for_blank_query()
    {
        (await service.SearchLocationsAsync("")).Should().BeEmpty();
        (await service.SearchLocationsAsync("   ")).Should().BeEmpty();
    }

    [Fact]
    public async Task SearchLocationsAsync_matches_case_insensitively()
    {
        var matches = await service.SearchLocationsAsync("cluj");

        matches.Should().NotBeEmpty();
        matches.Should().Contain(loc => loc.Contains("Cluj-Napoca", StringComparison.Ordinal));
    }
}
