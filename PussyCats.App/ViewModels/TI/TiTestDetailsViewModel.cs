using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels.TI;

public partial class TiTestDetailsViewModel : DispatchableObservableObject
{
    private readonly SessionContext session;

    [ObservableProperty] private int testId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string category = string.Empty;
    [ObservableProperty] private string questionTypeLabel = string.Empty;
    [ObservableProperty] private DateTime createdAt;
    [ObservableProperty] private bool isCompanyMode;

    public TiTestDetailsViewModel(SessionContext session)
    {
        this.session = session;
    }

    public void Load(TiTestCardViewModel card)
    {
        TestId = card.TestId;
        Title = card.Title;
        Category = card.Category;
        QuestionTypeLabel = card.QuestionTypeLabel;
        CreatedAt = card.CreatedAt;
        IsCompanyMode = session.Mode == AppMode.Company;
    }
}
