using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserServiceTests
{
    private readonly FakeUserRepository repo = new();
    private readonly UserService service;

    public UserServiceTests()
    {
        service = new UserService(repo);
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsCorrectUser()
    {
        repo.Seed(new UserBuilder().WithId(1).WithEmail("a@b.test").Build());

        var user = await service.GetByIdAsync(1);

        user.Should().NotBeNull();
        user!.Email.Should().Be("a@b.test");
    }

    [Fact]
    public async Task GetByIdAsync_UserIsMissing_ReturnsNull()
    {
        (await service.GetByIdAsync(99)).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleUsersExist_ReturnsEveryUser()
    {
        repo.Seed(
            new UserBuilder().WithId(1).Build(),
            new UserBuilder().WithId(2).Build(),
            new UserBuilder().WithId(3).Build());

        (await service.GetAllAsync()).Should().HaveCount(3);
    }

    [Fact]
    public async Task AddAsync_NewUserProvided_PersistsUserAndAssignsId()
    {
        var user = new UserBuilder().WithId(0).WithEmail("new@user.test").Build();

        var saved = await service.AddAsync(user);

        saved.UserId.Should().BeGreaterThan(0);
        (await service.GetByIdAsync(saved.UserId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingUserModified_ReplacesUserInStore()
    {
        var user = new UserBuilder().WithId(1).WithEmail("old@test.com").Build();
        repo.Seed(user);
        user.Email = "new@test.com";

        await service.UpdateAsync(user);

        (await service.GetByIdAsync(1))!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task RemoveAsync_UserExists_DeletesUserFromStore()
    {
        repo.Seed(new UserBuilder().WithId(1).Build());

        await service.RemoveAsync(1);

        (await service.GetByIdAsync(1)).Should().BeNull();
    }
}