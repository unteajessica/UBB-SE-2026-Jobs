using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class OurJobsPage : Page
    {
        public OurJobsViewModel ViewModel { get; }

        public OurJobsPage()
        {
            this.InitializeComponent();
            var mainWindow = App.MainWindow;
            ViewModel = new OurJobsViewModel(mainWindow.JobsService);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadJobsAsync();
        }

        private void CreateJobButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateJobPage));
        }

        private void ViewApplicantsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is JobPosting job)
            {
                Frame.Navigate(typeof(JobApplicantsPage), job);
            }
        }

        private void PayToPromoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is JobPosting job)
            {
                var paymentWindow = new PaymentWindow(job.JobId);
                paymentWindow.Activate();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
