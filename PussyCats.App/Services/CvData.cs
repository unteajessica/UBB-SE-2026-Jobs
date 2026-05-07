namespace PussyCats.App.Services;

public class CvData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? University { get; set; }
    public int ExpectedGraduationYear { get; set; }
    public string? GitHub { get; set; }
    public string? LinkedIn { get; set; }
    public string? Address { get; set; }
    public string? Motivation { get; set; }
    public bool HasDisabilities { get; set; }
    public List<string>? Skills { get; set; }
    public List<CvWorkExperience>? WorkExperiences { get; set; }
    public List<CvProject>? Projects { get; set; }
    public List<CvActivity>? ExtraCurricularActivities { get; set; }
}

public class CvWorkExperience
{
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Description { get; set; }
    public bool CurrentlyWorking { get; set; }
}

public class CvProject
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Technologies { get; set; }
    public string? Url { get; set; }
}

public class CvActivity
{
    public string? ActivityName { get; set; }
    public string? Organization { get; set; }
    public string? Role { get; set; }
    public string? Period { get; set; }
    public string? Description { get; set; }
}
