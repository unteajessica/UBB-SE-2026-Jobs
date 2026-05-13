using FluentAssertions;
using PussyCats_App.Services.CvParsingService;

namespace PussyCats.Tests.Services;

public class CvParsingServiceTests
{
    private readonly CvParsingService service = new();

    private const int MaxSkills = 30;

    private const string FirstName = "Ada";
    private const string LastName = "Lovelace";
    private const int Age = 28;
    private const string Gender = "F";
    private const string NormalizedGender = "Female";
    private const string Email = "ADA@EXAMPLE.COM";
    private const string NormalizedEmail = "ada@example.com";
    private const string PhoneNumber = "+40 (123) 456-789";
    private const string PhonePrefix = "+40";
    private const string Country = "Romania";
    private const string City = "Cluj-Napoca";
    private const string University = "Cambridge";
    private const int GraduationYear = 9999;
    private const string GitHub = "github.com/ada";
    private const string LinkedIn = "linkedin.com/in/ada";
    private const string Address = "1 Main St";
    private const string Motivation = "Build cool things.";

    private static readonly string CvDataJson = $$"""
        {
        "FirstName": "{{FirstName}}",
        "LastName": "{{LastName}}",
        "Age": {{Age}},
        "Gender": "{{Gender}}",
        "Email": "{{Email}}",
        "PhoneNumber": "{{PhoneNumber}}",
        "Country": "{{Country}}",
        "City": "{{City}}",
        "University": "{{University}}",
        "ExpectedGraduationYear": {{GraduationYear}},
        "GitHub": "{{GitHub}}",
        "LinkedIn": "{{LinkedIn}}",
        "Address": "{{Address}}",
        "Motivation": "{{Motivation}}",
        "HasDisabilities": false,
        "Skills": ["C#", "C#", "Python"]
        }
        """;


    [Fact]
    public void ParseCvFile_UnsupportedExtensionProvided_ThrowsException()
    {
        Action unsupportedFileType = () => service.ParseCvFile("ignored", ".txt");

        unsupportedFileType.Should().Throw<Exception>().WithMessage("*Unsupported file type*");
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_ParsesFirstName()
    {
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.FirstName.Should().Be(FirstName);
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_ParsesLastName()
    {
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.LastName.Should().Be(LastName);
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_ParsesAge()
    {
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.Age.Should().Be(Age);
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_NormalizesEmail()
    {
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.Email.Should().Be(NormalizedEmail);
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_NormalizesPhone()
    {
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.Phone.Should().StartWith(PhonePrefix);
    }

    [Fact]
    public void ParseCvFile_ValidJsonFormatProvided_DeduplicatesSkills()
    {
        var skillsExpectedCount = 2;
        var user = service.ParseCvFile(CvDataJson, ".json");

        user.Skills.Should().HaveCount(skillsExpectedCount);
    }

    [Theory]
    [InlineData("M", "Male")]
    [InlineData("F", "Female")]
    [InlineData("Other", "")]
    public void ParseCvFile_GenderCodes_NormalizesGenderStrings(string inputGender, string expectedGender)
    {
        var cvDataJson = $$"""
    {
        "FirstName": "X",
        "Gender": "{{inputGender}}"
    }
    """;
        var user = service.ParseCvFile(cvDataJson, ".json");

        user.Gender.Should().Be(expectedGender);
    }

    [Fact]
    public void ParseCvFile_AgeOutsideValidRange_ZerosAgeField()
    {
        var cvDataInvalidAge = """{ "FirstName": "X", "Age": 5 }""";

        var user = service.ParseCvFile(cvDataInvalidAge, ".json");

        user.Age.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_GraduationYearTooFarInFuture_ZerosGraduationYearField()
    {

        var user = service.ParseCvFile(CvDataJson, ".json");

        user.ExpectedGraduationYear.Should().Be(0);
    }

    [Fact]
    public void ParseCvFile_TooManySkillsProvided_CapsSkillCountAtMaxSkills()
    {
        var skillsArray = string.Join(",", Enumerable.Range(1, 50).Select(skillNumber => $"\"Skill{skillNumber}\""));
        var cvData = $"{{ \"FirstName\": \"X\", \"Skills\": [{skillsArray}] }}";

        var user = service.ParseCvFile(cvData, ".json");

        user.Skills.Should().HaveCount(MaxSkills);
    }

    [Fact]
    public void ParseCvFile_MotivationExceedsLimit_TruncatesMotivationAtMaxMotivtionLengthChars()
    {
        int motivationLength = 1500;
        int maxMotivationLength = 1000;

        var motivation = new string('x', motivationLength);
        var cvData = $"{{ \"FirstName\": \"X\", \"Motivation\": \"{motivation}\" }}";

        var user = service.ParseCvFile(cvData, ".json");

        user.Motivation.Length.Should().Be(maxMotivationLength);
    }

    [Fact]
    public void ParseCvFile_InvalidEmailProvided_BlanksEmailField()
    {
        var cvDataInvalidEmail = """{ "FirstName": "X", "Email": "not-an-email" }""";

        var user = service.ParseCvFile(cvDataInvalidEmail, ".json");

        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void ParseCvFile_MalformedJsonProvided_ThrowsException()
    {
        Action formatNotJsonFailedToParseCv = () => service.ParseCvFile("{ not json", ".json");

        formatNotJsonFailedToParseCv.Should().Throw<Exception>().WithMessage("*Failed to parse CV file*");
    }



}