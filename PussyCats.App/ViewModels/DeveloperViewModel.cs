using CommunityToolkit.Mvvm.ComponentModel;

namespace PussyCats.App.ViewModels;

public class DeveloperViewModel : DispatchableObservableObject
{
    public string ModeSummary { get; } = "Developer mode";

    public IReadOnlyList<DeveloperPostViewModel> Posts { get; } =
    [
        new("Backend integration", "API and database work are connected through repository proxies. File upload and profile saving now use the shared API surface."),
        new("Candidate experience", "Profiles, documents, skill tests, recommendations, applications, and CV export live in the user workflow."),
        new("Company workflow", "Review Applicants and Applicant Status use the company session context and the merged match status pipeline."),
    ];
}

public sealed record DeveloperPostViewModel(string Title, string Description);
