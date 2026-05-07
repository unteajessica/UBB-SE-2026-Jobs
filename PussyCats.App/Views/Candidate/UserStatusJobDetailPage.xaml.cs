using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Views.Candidate;

public sealed partial class UserStatusJobDetailPage : Page
{
    public UserStatusJobDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not ApplicationCardModel card)
        {
            PageTitleText.Text = "Job Details";
            return;
        }

        PageTitleText.Text  = "Job Details";
        CompanyText.Text    = card.CompanyName;
        ScoreText.Text      = card.FormattedScore;
        DescriptionText.Text = card.JobDescription;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
        else Frame.Navigate(typeof(UserStatusPage));
    }
}
