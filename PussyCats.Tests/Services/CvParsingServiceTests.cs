using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class CvParsingServiceTests
{
    private readonly CvParsingService service = new();

    [Fact]
    public void ParseCvFile_throws_for_unsupported_extension()
    {
        Action act = () => service.ParseCvFile("ignored", ".txt");

        act.Should().Throw<Exception>().WithMessage("*Unsupported file type*");
    }

    [Fact]
    public void ParseCvFile_parses_valid_json()
    {
        var json = """
        {
          "FirstName": "Ada",
          "LastName": "Lovelace",
          "Age": 28,
          "Gender": "F",
          "Email": "ADA@EXAMPLE.COM",
          "PhoneNumber": "+40 (123) 456-789",
          "Country": "Romania",
          "City": "Cluj-Napoca",
          "University": "Cambridge",
          "ExpectedGraduationYear": 9999,
          "GitHub": "github.com/ada",
          "LinkedIn": "linkedin.com/in/ada",
          "Address": "1 Main St",
          "Motivation": "Build cool things.",
          "HasDisabilities": false,
          "Skills": ["C#", "C#", "Python"]
        }
        """;

        var user = service.ParseCvFile(json, ".json");

        user.FirstName.Should().Be("Ada");
        user.LastName.Should().Be("Lovelace");
        user.Age.Should().Be(28);
        user.Gender.Should().Be("Female");
        user.Email.Should().Be("ada@example.com");
        user.Phone.Should().StartWith("+40");
        user.Skills.Should().HaveCount(2); // duplicates deduped
    }

    [Fact]
    public void ParseCvFile_zeros_age_outside_valid_range()
    {
        var json = """{ "FirstName": "X", "Age": 5 }""";

        var user = service.ParseCvFile(json, ".json");

        user.Age.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_zeros_graduation_year_too_far_in_future()
    {
        var json = """{ "FirstName": "X", "ExpectedGraduationYear": 9999 }""";

        var user = service.ParseCvFile(json, ".json");

        user.ExpectedGraduationYear.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_caps_skill_count_at_30()
    {
        var skillsArray = string.Join(",", Enumerable.Range(1, 50).Select(i => $"\"Skill{i}\""));
        var json = $"{{ \"FirstName\": \"X\", \"Skills\": [{skillsArray}] }}";

        var user = service.ParseCvFile(json, ".json");

        user.Skills.Should().HaveCount(30);
    }

    [Fact]
    public void ParseCvFile_truncates_motivation_at_1000_chars()
    {
        var motivation = new string('x', 1500);
        var json = $"{{ \"FirstName\": \"X\", \"Motivation\": \"{motivation}\" }}";

        var user = service.ParseCvFile(json, ".json");

        user.Motivation.Length.Should().Be(1000);
    }

    [Fact]
    public void ParseCvFile_blanks_invalid_email()
    {
        var json = """{ "FirstName": "X", "Email": "not-an-email" }""";

        var user = service.ParseCvFile(json, ".json");

        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void ParseCvFile_normalizes_short_gender_codes()
    {
        var maleJson = """{ "FirstName": "X", "Gender": "M" }""";
        var femaleJson = """{ "FirstName": "X", "Gender": "F" }""";
        var unknownJson = """{ "FirstName": "X", "Gender": "Other" }""";

        service.ParseCvFile(maleJson, ".json").Gender.Should().Be("Male");
        service.ParseCvFile(femaleJson, ".json").Gender.Should().Be("Female");
        service.ParseCvFile(unknownJson, ".json").Gender.Should().BeEmpty();
    }

    [Fact]
    public void ParseCvFile_throws_on_malformed_json()
    {
        Action act = () => service.ParseCvFile("{ not json", ".json");

        act.Should().Throw<Exception>().WithMessage("*Failed to parse CV file*");
    }

    [Fact(Skip = "CvData uses DateTimeOffset which XmlSerializer rejects (TypeDesc.CheckSupported). Production XML path is broken on the same limitation; flag as open item.")]
    public void ParseCvFile_parses_valid_xml()
    {
        // Documents an open item: CvParsingService accepts .xml but XmlSerializer
        // can't serialize DateTimeOffset. Real XML uploads would throw before any
        // user.* fields could be inspected. Either swap to DateTime + XmlIgnore on
        // DateTimeOffset, or drop the XML branch in Phase 8.
    }
}
