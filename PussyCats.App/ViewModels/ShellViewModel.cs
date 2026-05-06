using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels;

public class ShellViewModel : ObservableObject
{
    private readonly SessionContext session;
    private string activePage = "MyStatus";

    public ShellViewModel(SessionContext session)
    {
        this.session = session;
        RecommendationsCommand = new RelayCommand(() => ActivePage = "Recommendations");
        MyStatusCommand = new RelayCommand(() => ActivePage = "MyStatus");
        ChatCommand = new RelayCommand(() => ActivePage = "Chat");
        CandidateModeCommand = new RelayCommand(() => Mode = AppMode.Candidate);
        CompanyModeCommand = new RelayCommand(() => Mode = AppMode.Company);
    }

    public ICommand RecommendationsCommand { get; }
    public ICommand MyStatusCommand { get; }
    public ICommand ChatCommand { get; }
    public ICommand CandidateModeCommand { get; }
    public ICommand CompanyModeCommand { get; }

    public string ActivePage
    {
        get => activePage;
        set
        {
            if (SetProperty(ref activePage, value))
            {
                OnPropertyChanged(nameof(IsRecommendationsActive));
                OnPropertyChanged(nameof(IsMyStatusActive));
                OnPropertyChanged(nameof(IsChatActive));
            }
        }
    }

    public AppMode Mode
    {
        get => session.Mode;
        set
        {
            if (session.Mode != value)
            {
                session.Mode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCandidateMode));
                OnPropertyChanged(nameof(IsCompanyMode));
            }
        }
    }

    public bool IsRecommendationsActive => ActivePage == "Recommendations";
    public bool IsMyStatusActive => ActivePage == "MyStatus";
    public bool IsChatActive => ActivePage == "Chat";
    public bool IsCandidateMode => Mode == AppMode.Candidate;
    public bool IsCompanyMode => Mode == AppMode.Company;
}
