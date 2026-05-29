using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.ViewModels
{
    public partial class JobApplicantsViewModel : ObservableObject
    {
        private const string StatusAccepted = "Accepted";
        private const string StatusRejected = "Rejected";
        private const string StatusOnHold = "On Hold";
        private const string StatusRecommendation = "Recommandation";
        private const string StatusPending = "Pending";

        private const string DefaultApplicantName = "Applicant";
        private const string DefaultJobTitle = "the position";
        private const string DefaultCompanyName = "Our company";

        private const string EmailSubject = "Application status update";
        private const string SmtpHostAddress = "smtp.gmail.com";
        private const int SmtpHostPort = 587;
        private const int SmtpTimeoutMilliseconds = 60000;
        private const string AdminEmailAddress = "carla.draghiciu@cnglsibiu.ro";
        private const string AdminEmailPassword = "[REDACTED_PASSWORD]";

        private const string ErrorNoSession = "No company session; cannot send mail.";
        private const string ErrorNoApplicant = "No applicant selected.";
        private const string ErrorNoEmail = "This applicant has no email address on file.";
        private const string ErrorInvalidCv = "Invalid CV";

        private const int MockApplicantId = 999;
        private const string MockApplicantName = "Mock Applicant (DB Error)";

        private readonly IApplicantService applicantService;
        private readonly SessionService? sessionService;

        public JobPosting SelectedJob { get; private set; }

        public ObservableCollection<Applicant> Applicants { get; } = new ObservableCollection<Applicant>();

        public ObservableCollection<string> ApplicationStatusOptions { get; } = new ObservableCollection<string>
        {
            StatusAccepted, StatusRejected, StatusOnHold, StatusRecommendation
        };

        [ObservableProperty] private string draftStatus = string.Empty;
        [ObservableProperty] private string draftAppTestGrade = string.Empty;
        [ObservableProperty] private string draftCvGrade = string.Empty;
        [ObservableProperty] private string draftCompanyTestGrade = string.Empty;
        [ObservableProperty] private string draftInterviewGrade = string.Empty;

        private string cvScanErrorMessage = string.Empty;
        public string CvScanErrorMessage
        {
            get => cvScanErrorMessage;
            private set
            {
                if (SetProperty(ref cvScanErrorMessage, value ?? string.Empty))
                {
                    OnPropertyChanged(nameof(CvScanErrorVisibility));
                }
            }
        }

        public Visibility CvScanErrorVisibility =>
            string.IsNullOrEmpty(cvScanErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

        private bool isCvScanning;
        public bool IsCvScanning
        {
            get => isCvScanning;
            private set
            {
                if (SetProperty(ref isCvScanning, value))
                {
                    OnPropertyChanged(nameof(CanScanCv));
                }
            }
        }

        public bool CanScanCv => SelectedApplicant != null && !IsCvScanning;

        private Applicant? selectedApplicant;
        public Applicant? SelectedApplicant
        {
            get => selectedApplicant;
            set
            {
                if (SetProperty(ref selectedApplicant, value))
                {
                    if (selectedApplicant != null)
                    {
                        LoadDraft(selectedApplicant);
                        DetailsVisibility = Visibility.Visible;
                        TableVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DetailsVisibility = Visibility.Collapsed;
                        TableVisibility = Visibility.Visible;
                    }
                    OnPropertyChanged(nameof(CanScanCv));
                }
            }
        }

        private Visibility tableVisibility = Visibility.Visible;

        public Visibility TableVisibility
        {
            get => tableVisibility;
            set => SetProperty(ref tableVisibility, value);
        }

        private Visibility detailsVisibility = Visibility.Collapsed;

        public Visibility DetailsVisibility
        {
            get => detailsVisibility;
            set => SetProperty(ref detailsVisibility, value);
        }

        public JobApplicantsViewModel(JobPosting job, IApplicantService applicantService, SessionService? sessionService)
        {
            SelectedJob = job;
            this.applicantService = applicantService;
            this.sessionService = sessionService;

            LoadApplicants();
        }

        public async Task<(bool Ok, string Message)> SendStatusMailAsync()
        {
            if (sessionService?.LoggedInUser == null)
            {
                return (false, ErrorNoSession);
            }

            if (SelectedApplicant?.User == null)
            {
                return (false, ErrorNoApplicant);
            }

            var email = SelectedApplicant.User.Email?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                return (false, ErrorNoEmail);
            }

            var statusForMail = string.IsNullOrWhiteSpace(DraftStatus) ? StatusPending : DraftStatus;
            var jobTitle = SelectedJob?.JobTitle ?? DefaultJobTitle;
            var sourceName = sessionService.LoggedInUser.Name ?? DefaultCompanyName;
            var applicantName = string.IsNullOrWhiteSpace(SelectedApplicant.User.Name) ? DefaultApplicantName : SelectedApplicant.User.Name;

            var fromAddress = new MailAddress(AdminEmailAddress, sourceName);
            var toAddress = new MailAddress(email, applicantName);
            string bodyText =
                $"Hello {applicantName},\n\n" +
                $"Your application status for \"{jobTitle}\" at {sourceName} is: {statusForMail}.\n\n" +
                "If you have questions, please reply to this email.\n";

            try
            {
                var smtpClient = new SmtpClient
                {
                    Host = SmtpHostAddress,
                    Port = SmtpHostPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, AdminEmailPassword),
                    Timeout = SmtpTimeoutMilliseconds
                };

                using (var mailMessage = new MailMessage(fromAddress, toAddress)
                {
                    Subject = EmailSubject,
                    Body = bodyText
                })
                {
                    await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(true);
                }

                return (true, $"Status \"{statusForMail}\" was sent to {email}.");
            }
            catch (Exception exception)
            {
                return (false, exception.Message);
            }
        }

        private async void LoadApplicants()
        {
            Applicants.Clear();
            if (SelectedJob != null)
            {
                try
                {
                    var applicants = await applicantService.GetApplicantsForJob(SelectedJob);
                    foreach (var applicant in applicants)
                    {
                        Applicants.Add(applicant);
                    }
                }
                catch (Exception)
                {
                    Applicants.Add(new Applicant { ApplicantId = MockApplicantId, User = new User(1, MockApplicantName, string.Empty), ApplicationStatus = StatusOnHold, Job = SelectedJob });
                }
            }
        }

        private void LoadDraft(Applicant applicant)
        {
            CvScanErrorMessage = string.Empty;
            DraftStatus = applicant.ApplicationStatus;
            DraftAppTestGrade = applicant.AppTestGrade?.ToString() ?? string.Empty;
            DraftCvGrade = applicant.CvGrade?.ToString() ?? string.Empty;
            DraftCompanyTestGrade = applicant.CompanyTestGrade?.ToString() ?? string.Empty;
            DraftInterviewGrade = applicant.InterviewGrade?.ToString() ?? string.Empty;
        }

        public async Task ScanCvAsync()
        {
            if (SelectedApplicant == null || IsCvScanning)
            {
                return;
            }

            CvScanErrorMessage = string.Empty;
            IsCvScanning = true;
            try
            {
                var applicant = SelectedApplicant;
                decimal? grade = await applicantService.ScanCvXmlAsync(applicant);

                if (grade.HasValue)
                {
                    DraftCvGrade = grade.Value.ToString(CultureInfo.InvariantCulture);
                    CvScanErrorMessage = string.Empty;
                }
                else
                {
                    CvScanErrorMessage = ErrorInvalidCv;
                }
            }
            finally
            {
                IsCvScanning = false;
            }
        }

        public void SaveChanges()
        {
            if (SelectedApplicant == null)
            {
                return;
            }

            SelectedApplicant.ApplicationStatus = DraftStatus;

            if (decimal.TryParse(DraftAppTestGrade, out decimal parsedAppTest))
            {
                SelectedApplicant.AppTestGrade = parsedAppTest;
            }
            else if (string.IsNullOrWhiteSpace(DraftAppTestGrade))
            {
                SelectedApplicant.AppTestGrade = null;
            }
            if (decimal.TryParse(DraftCvGrade, out decimal parsedCvGrade))
            {
                SelectedApplicant.CvGrade = parsedCvGrade;
            }
            else if (string.IsNullOrWhiteSpace(DraftCvGrade))
            {
                SelectedApplicant.CvGrade = null;
            }
            if (decimal.TryParse(DraftCompanyTestGrade, out decimal parsedCompanyTest))
            {
                SelectedApplicant.CompanyTestGrade = parsedCompanyTest;
            }
            else if (string.IsNullOrWhiteSpace(DraftCompanyTestGrade))
            {
                SelectedApplicant.CompanyTestGrade = null;
            }
            if (decimal.TryParse(DraftInterviewGrade, out decimal parsedInterview))
            {
                SelectedApplicant.InterviewGrade = parsedInterview;
            }
            else if (string.IsNullOrWhiteSpace(DraftInterviewGrade))
            {
                SelectedApplicant.InterviewGrade = null;
            }
            applicantService.UpdateApplicant(SelectedApplicant);

            int index = Applicants.IndexOf(SelectedApplicant);
            if (index >= 0)
            {
                Applicants[index] = SelectedApplicant;
            }

            GoBackFromDetails();
        }

        public void GoBackFromDetails()
        {
            SelectedApplicant = null;
        }

        public void RemoveApplicant(Applicant applicant)
        {
            if (applicant != null)
            {
                applicantService.RemoveApplicant(applicant.ApplicantId);
                Applicants.Remove(applicant);
                if (SelectedApplicant == applicant)
                {
                    GoBackFromDetails();
                }
            }
        }
    }
}
