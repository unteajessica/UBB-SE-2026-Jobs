namespace Tests_and_Interviews
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Validators;
    using Tests_and_Interviews.Views;

    public sealed partial class MainWindow : Window
    {
        private const int DefaultCompanyId = 1;

        public Frame RootFrame => rootFrame;

        public IEventsService EventsService { get; }

        public ICompanyService CompanyService { get; }

        public SessionService SessionService { get; }

        public ICollaboratorsService CollabsService { get; }

        public IJobsService JobsService { get; }

        public IApplicantService ApplicantService { get; }

        public IGameService GameService { get; }

        public IPaymentService PaymentService { get; }

        public IGameValidator GameValidator { get; }

        public IEventValidator EventValidator { get; }

        public ICompanyValidator CompanyValidator { get; }

        public IPaymentValidator PaymentValidator { get; }

        public MainWindow()
        {
            this.CompanyService = new CompanyService();
            this.GameService = new GameService(DefaultCompanyId);

            this.CollabsService = new CollaboratorsService();

            Company defaultCompany = new Company("ndj", "dnis", "dnjs", "hdjd", "sybau", "dj@");

            this.InitializeComponent();

            this.EventsService = new EventsService();
            this.SessionService = new SessionService(defaultCompany);
            this.JobsService = new JobsService();
            this.ApplicantService = new ApplicantService();
            this.CompanyValidator = new CompanyValidator();
            this.EventValidator = new EventValidator();
            this.PaymentValidator = new PaymentValidator();
            this.GameValidator = new GameValidator();

            this.PaymentService = new PaymentService(this.PaymentValidator);
        }

        public void ReturnToMainMenu()
        {
            this.RootFrame.Content = null;
            this.RootFrame.BackStack.Clear();
            this.MainMenuContainer.Visibility = Visibility.Visible;
        }

        private void NavigateToViewProfile_Click(object sender, RoutedEventArgs e)
        {
            MainMenuContainer.Visibility = Visibility.Collapsed;
            this.RootFrame.Navigate(typeof(ViewProfilePage), DefaultCompanyId);
        }

        private void NavigateToMainTest_Click(object sender, RoutedEventArgs e)
        {
            MainMenuContainer.Visibility = Visibility.Collapsed;
            this.RootFrame.Navigate(typeof(MainTestPage));
        }

        private void NavigateToRecruiter_Click(object sender, RoutedEventArgs e)
        {
            MainMenuContainer.Visibility = Visibility.Collapsed;
            this.RootFrame.Navigate(typeof(RecruiterPage));
        }

        private void NavigateToCandidateHome_Click(object sender, RoutedEventArgs e)
        {
            MainMenuContainer.Visibility = Visibility.Collapsed;
            this.RootFrame.Navigate(typeof(CandidateHomePage));
        }
    }
}