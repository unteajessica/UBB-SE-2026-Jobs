using FluentAssertions;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Smoke;

public class SolutionLoadsTest
{
    [Fact]
    public void Every_fake_and_builder_can_be_constructed()
    {
        new FakeUserRepository().Should().NotBeNull();
        new FakeJobRepository().Should().NotBeNull();
        new FakeCompanyRepository().Should().NotBeNull();
        new FakeMatchRepository().Should().NotBeNull();
        new FakeDocumentRepository().Should().NotBeNull();
        new FakeSkillRepository().Should().NotBeNull();
        new FakeJobSkillRepository().Should().NotBeNull();
        new FakeUserSkillRepository().Should().NotBeNull();
        new FakeSkillGroupRepository().Should().NotBeNull();
        new FakeSkillTestRepository().Should().NotBeNull();
        new FakePersonalityTestRepository().Should().NotBeNull();
        new FakeRecommendationRepository().Should().NotBeNull();

        new UserBuilder().Build().Should().NotBeNull();
        new JobBuilder().Build().Should().NotBeNull();
        new MatchBuilder().Build().Should().NotBeNull();
        new CompanyBuilder().Build().Should().NotBeNull();
        new SkillBuilder().Build().Should().NotBeNull();
        new SkillTestBuilder().Build().Should().NotBeNull();
        new PersonalityResultBuilder().Build().Should().NotBeNull();
    }
}
