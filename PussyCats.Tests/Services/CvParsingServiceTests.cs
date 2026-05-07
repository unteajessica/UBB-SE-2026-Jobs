using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class CvParsingServiceTests
{
    private readonly CvParsingService service = new();

    [Fact]
    public void ParseCvFile_UnsupportedExtensionProvided_ThrowsException()
    {
        Action act = () => service.ParseCvFile("ignored", ".txt");

        act.Should().Throw<Exception>().WithMessage("*Unsupported file type*");
    }

    [Fact]
    public void ParseCvFile_ValidJsonProvided_ParsesFieldsCorrectly()
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
    public void ParseCvFile_AgeOutsideValidRange_ZerosAgeField()
    {
        var json = """{ "FirstName": "X", "Age": 5 }""";

        var user = service.ParseCvFile(json, ".json");

        user.Age.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_GraduationYearTooFarInFuture_ZerosGraduationYearField()
    {
        var json = """{ "FirstName": "X", "ExpectedGraduationYear": 9999 }""";

        var user = service.ParseCvFile(json, ".json");

        user.ExpectedGraduationYear.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_TooManySkillsProvided_CapsSkillCountAt30()
    {
        var skillsArray = string.Join(",", Enumerable.Range(1, 50).Select(i => $"\"Skill{i}\""));
        var json = $"{{ \"FirstName\": \"X\", \"Skills\": [{skillsArray}] }}";

        var user = service.ParseCvFile(json, ".json");

        user.Skills.Should().HaveCount(30);
    }

    [Fact]
    public void ParseCvFile_MotivationExceedsLimit_TruncatesMotivationAt1000Chars()
    {
        var motivation = new string('x', 1500);
        var json = $"{{ \"FirstName\": \"X\", \"Motivation\": \"{motivation}\" }}";

        var user = service.ParseCvFile(json, ".json");

        user.Motivation.Length.Should().Be(1000);
    }

    [Fact]
    public void ParseCvFile_InvalidEmailProvided_BlanksEmailField()
    {
        var json = """{ "FirstName": "X", "Email": "not-an-email" }""";

        var user = service.ParseCvFile(json, ".json");

        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void ParseCvFile_ShortGenderCodesProvided_NormalizesGenderStrings()
    {
        var maleJson = """{ "FirstName": "X", "Gender": "M" }""";
        var femaleJson = """{ "FirstName": "X", "Gender": "F" }""";
        var unknownJson = """{ "FirstName": "X", "Gender": "Other" }""";

        service.ParseCvFile(maleJson, ".json").Gender.Should().Be("Male");
        service.ParseCvFile(femaleJson, ".json").Gender.Should().Be("Female");
        service.ParseCvFile(unknownJson, ".json").Gender.Should().BeEmpty();
    }

    [Fact]
    public void ParseCvFile_MalformedJsonProvided_ThrowsException()
    {
        Action act = () => service.ParseCvFile("{ not json", ".json");

        act.Should().Throw<Exception>().WithMessage("*Failed to parse CV file*");
    }

    [Fact]
    public void ParseCvFile_XmlFormatProvided_ThrowsUnsupportedException()
    {
        Action act = () => service.ParseCvFile("<CvData />", ".xml");

        act.Should().Throw<Exception>().WithMessage("*Only JSON is supported*");
    }
}