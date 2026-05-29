using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Data;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Views;

namespace Tests_and_Interviews
{
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; }

        private Window? _window;

        public static int CurrentUserId { get; private set; } = 0;

        private readonly string _connectionString = "Data Source=LucaT2\\MSSQLSERVER01;Initial Catalog=TestsAndInterviews;Integrated Security=True";

        public App()
        {
            InitializeComponent();

            var userService = new UserService();
            using var context = new AppDbContext();
            bool canConnect = context.Database.CanConnect();

            try
            {
                var users = Task.Run(() => userService.GetAllAsync()).Result;
                var alice = users.FirstOrDefault(u => u.Name == "Alice Johnson");

                CurrentUserId = alice?.Id ?? 0;
                System.Diagnostics.Debug.WriteLine($"[App] CurrentUserId = {CurrentUserId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Failed to fetch users from database. Did you run the SQL seed script? Error: {ex.Message}");
                CurrentUserId = 0;
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();

            /*

            // Main test window
            _window = new MainWindow();
            _window.Activate();

            // Recruiter window
            var recruiterWindow = new Window();
            var recruiterFrame = new Frame();
            recruiterFrame.Navigate(typeof(RecruiterPage));
            recruiterWindow.Content = recruiterFrame;
            recruiterWindow.Title = "Recruiter";
            recruiterWindow.Activate();

            // Candidate home window
            var candidateWindow = new Window();
            var candidateFrame = new Frame();
            candidateFrame.Navigate(typeof(CandidateHomePage));
            candidateWindow.Content = candidateFrame;
            candidateWindow.Title = "Candidate Home";
            candidateWindow.Activate();*/
        }
    }
}