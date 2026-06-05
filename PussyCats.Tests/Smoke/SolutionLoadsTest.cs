using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Smoke;

public class SolutionLoadsTest
{
    [Fact]
    public void Every_fake_and_builder_can_be_constructed()
    {
        Assert.NotNull(new FakeUserRepository());
        Assert.NotNull(new FakeJobRepository());
        Assert.NotNull(new FakeCompanyRepository());
        Assert.NotNull(new FakeMatchRepository());
        Assert.NotNull(new FakeDocumentRepository());
        Assert.NotNull(new FakeSkillRepository());
        Assert.NotNull(new FakeJobSkillRepository());
        Assert.NotNull(new FakeUserSkillRepository());
        Assert.NotNull(new FakeSkillGroupRepository());
        Assert.NotNull(new FakeSkillTestRepository());
        Assert.NotNull(new FakePersonalityTestRepository());
        Assert.NotNull(new FakeRecommendationRepository());

        Assert.NotNull(new UserBuilder().Build());
        Assert.NotNull(new JobBuilder().Build());
        Assert.NotNull(new MatchBuilder().Build());
        Assert.NotNull(new CompanyBuilder().Build());
        Assert.NotNull(new SkillBuilder().Build());
        Assert.NotNull(new SkillTestBuilder().Build());
        Assert.NotNull(new PersonalityResultBuilder().Build());
    }
}
