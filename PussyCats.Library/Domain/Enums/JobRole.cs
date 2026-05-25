using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobRole
{
    FrontendDeveloper,
    BackendDeveloper,
    UiUxDesigner,
    DevOpsEngineer,
    ProjectManager,
    DataAnalyst,
    CybersecuritySpecialist,
    AiMlEngineer,
}
