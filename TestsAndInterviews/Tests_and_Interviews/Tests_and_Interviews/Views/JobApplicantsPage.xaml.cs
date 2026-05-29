using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class JobApplicantsPage : Page
    {
        private const string DialogTitleSuccess = "Email sent";
        private const string DialogTitleError = "Could not send email";
        private const string DialogButtonOk = "OK";

        public JobApplicantsViewModel ViewModel { get; private set; }

        public JobApplicantsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is JobPosting job)
            {
                ViewModel = new JobApplicantsViewModel(job, App.MainWindow.ApplicantService, App.MainWindow?.SessionService);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.TableVisibility == Visibility.Collapsed)
            {
                ViewModel.GoBackFromDetails();
            }
            else if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ApplicantsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem is Applicant applicant)
            {
                ViewModel.SelectedApplicant = applicant;
            }
        }

        private void RemoveApplicant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Applicant applicant)
            {
                ViewModel.RemoveApplicant(applicant);
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveChanges();
        }

        private async void ScanCv_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.ScanCvAsync();
            }
        }

        private async void SendStatusMail_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var (isSent, message) = await ViewModel.SendStatusMailAsync();
            var dialog = new ContentDialog
            {
                Title = isSent ? DialogTitleSuccess : DialogTitleError,
                Content = message,
                CloseButtonText = DialogButtonOk,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
