using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class CompanyServiceTests
{
    private readonly FakeCompanyRepository repo = new();
    private readonly CompanyService service;

    public CompanyServiceTests()
    {
        service = new CompanyService(repo);
    }

    [Fact]
    public async Task GetByIdAsync_CompanyExists_ReturnsCompany()
    {
        repo.Seed(new CompanyBuilder().WithId(1).WithName("Acme").Build());

        var company = await service.GetByIdAsync(1);

        company.Should().NotBeNull();
        company!.CompanyName.Should().Be("Acme");
    }

    [Fact]
    public async Task GetAllAsync_MultipleCompaniesExist_ReturnsEveryCompany()
    {
        repo.Seed(
            new CompanyBuilder().WithId(1).Build(),
            new CompanyBuilder().WithId(2).Build());

        (await service.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAsync_ValidCompanyProvided_PersistsAndAssignsId()
    {
        var company = new CompanyBuilder().WithId(0).Build();

        var saved = await service.AddAsync(company);

        saved.CompanyId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCompanyModified_ReplacesCompanyInStore()
    {
        var company = new CompanyBuilder().WithId(1).WithName("Old").Build();
        repo.Seed(company);
        company.CompanyName = "New";

        await service.UpdateAsync(company);

        (await service.GetByIdAsync(1))!.CompanyName.Should().Be("New");
    }

    [Fact]
    public async Task RemoveAsync_CompanyExists_DeletesCompanyFromStore()
    {
        repo.Seed(new CompanyBuilder().WithId(1).Build());

        await service.RemoveAsync(1);

        (await service.GetByIdAsync(1)).Should().BeNull();
    }
}