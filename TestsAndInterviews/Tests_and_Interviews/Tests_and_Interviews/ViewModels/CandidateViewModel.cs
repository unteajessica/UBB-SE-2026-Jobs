// <copyright file="CandidateViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Xaml;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Views;

    /// <summary>
    /// Represents the view model for a candidate, providing commands and properties to manage interview scheduling,
    /// slot selection, and session management in the user interface.
    /// </summary>
    public class CandidateViewModel : INotifyPropertyChanged
    {
        private const int VISIBLEDAYSCOUNT = 3;

        private readonly IBookingService bookingService;
        private readonly IInterviewSessionService interviewSessionService;
        private readonly INotificationService notificationService;

        private List<Slot> availableSlots;
        private List<Slot> availableDays;
        private ObservableCollection<CompanyPosting> matchedCompanies;
        private CompanyPosting? selectedCompany;
        private Slot? selectedSlot;
        private DateTime selectedDay;
        private int dayStartIndex = 0;
        private bool isBookingVisible;
        private ObservableCollection<InterviewSession> interviewSessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CandidateViewModel"/> class with injected services.
        /// </summary>
        /// <param name="bookingService">The booking service.</param>
        /// <param name="interviewSessionRepository">The interview session repository.</param>
        /// <param name="notificationService">The notification service.</param>
        public CandidateViewModel(
            IBookingService bookingService,
            IInterviewSessionService interviewSessionService,
            INotificationService notificationService
            )
        {
            this.bookingService = bookingService;
            this.interviewSessionService = interviewSessionService;
            this.notificationService = notificationService;

            this.availableSlots = new List<Slot>();
            this.availableDays = new List<Slot>();
            this.interviewSessions = new ObservableCollection<InterviewSession>();
            this.matchedCompanies = new ObservableCollection<CompanyPosting>();

            this.LoadAvailableSlotsCommand = new AsyncRelayCommand(execute: this.LoadAvailableSlotsCommandExecute);
            this.ScheduleInterviewCommand = new AsyncRelayCommand(execute: this.ScheduleInterviewCommandExecute);
            this.JoinInterviewCommand = new RelayCommand(execute: this.JoinInterviewCommandExecute);
            this.CancelInterviewCommand = new AsyncRelayCommand(execute: this.CancelInterviewCommandExecute);
            this.SelectDayForInterviewCommand = new RelayCommand(execute: this.SelectDayForInterviewCommandExecute);
            this.SelectSlotForInterviewCommand = new RelayCommand(execute: this.SelectSlotForInterviewCommandExecute);
            this.ConfirmInterviewCommand = new AsyncRelayCommand(execute: this.ConfirmInterviewCommandExecute);
            this.LoadNextDaysCommand = new RelayCommand(execute: this.LoadNextDaysCommandExecute);
            this.LoadPreviousDaysCommand = new RelayCommand(execute: this.LoadPreviousDaysCommandExecute);

            this.MatchedCompanies = this.GetMatchedCompanies();

            this.LoadInterviewSessionsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CandidateViewModel"/> class with default services and commands.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public CandidateViewModel()
            : this(
                new BookingService(),
                new InterviewSessionService(),
                new NotificationService(new WindowsToastNotifier())
                  )
        {
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command that loads available slots when executed.
        /// </summary>
        public ICommand LoadAvailableSlotsCommand { get; }

        /// <summary>
        /// Gets the command that begins the scheduling of an interview.
        /// </summary>
        public ICommand ScheduleInterviewCommand { get; }

        /// <summary>
        /// Gets the command that selects a day in the calendar or date picker control.
        /// </summary>
        public ICommand SelectDayForInterviewCommand { get; }

        /// <summary>
        /// Gets the command that selects a slot in the user interface.
        /// </summary>
        public ICommand SelectSlotForInterviewCommand { get; }

        /// <summary>
        /// Gets the command that confirms the booking on the currently selected timeslot.
        /// </summary>
        public ICommand ConfirmInterviewCommand { get; }

        /// <summary>
        /// Gets the command that advances the view or data to the next set of days.
        /// </summary>
        public ICommand LoadNextDaysCommand { get; }

        /// <summary>
        /// Gets the command that navigates to the previous set of days in the view.
        /// </summary>
        public ICommand LoadPreviousDaysCommand { get; }

        /// <summary>
        /// Gets the command that initiates the join meeting operation.
        /// </summary>
        public ICommand JoinInterviewCommand { get; }

        /// <summary>
        /// Gets the command that cancels the current operation.
        /// </summary>
        public ICommand CancelInterviewCommand { get; }

        /// <summary>
        /// Gets or sets the collection of available slots.
        /// </summary>
        public List<Slot> AvailableSlots
        {
            get => this.availableSlots;
            set
            {
                if (this.availableSlots != value)
                {
                    this.availableSlots = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the collection of available days represented by slot objects.
        /// </summary>
        public List<Slot> AvailableDays
        {
            get => this.availableDays;
            set
            {
                if (this.availableDays != value)
                {
                    this.availableDays = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.VisibleDays));
                }
            }
        }

        /// <summary>
        /// Gets the collection of days currently visible in the view, based on the configured start index and visible
        /// day count.
        /// </summary>
        public IEnumerable<Slot> VisibleDays
        {
            get
            {
                var days = this.AvailableDays ?? new List<Slot>();
                return days.Skip(this.dayStartIndex).Take(VISIBLEDAYSCOUNT).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the collection of companies that match the candidate's profile or search criteria.
        /// </summary>
        public ObservableCollection<CompanyPosting> MatchedCompanies
        {
            get => this.matchedCompanies;
            set
            {
                if (this.matchedCompanies != value)
                {
                    this.matchedCompanies = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the company currently selected by the user.
        /// </summary>
        public CompanyPosting? SelectedCompany
        {
            get => this.selectedCompany;
            set
            {
                this.selectedCompany = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected slot.
        /// </summary>
        public Slot? SelectedSlot
        {
            get => this.selectedSlot;
            set
            {
                this.selectedSlot = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected day.
        /// </summary>
        public DateTime SelectedDay
        {
            get => this.selectedDay;
            set
            {
                this.selectedDay = value;
                this.OnPropertyChanged();
                _ = this.LoadSlotsForSelectedDay();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the booking section is visible to the user.
        /// </summary>
        public bool IsBookingVisible
        {
            get => this.isBookingVisible;
            set
            {
                this.isBookingVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of interview sessions.
        /// </summary>
        public ObservableCollection<InterviewSession> InterviewSessions
        {
            get => this.interviewSessions;
            set
            {
                if (this.interviewSessions != value)
                {
                    this.interviewSessions = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event to notify listeners that a property value has changed.
        /// </summary>
        /// <param name="name">The name of the property that changed. This value can be null to indicate that all properties have changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SelectSlotForInterviewCommandExecute(object? obj)
        {
            if (obj is not Slot selectedSlot)
            {
                return;
            }

            foreach (var slot in this.AvailableSlots)
            {
                slot.IsSlotSelected = false;
            }

            selectedSlot.IsSlotSelected = true;
            this.SelectedSlot = selectedSlot;
            this.OnPropertyChanged(nameof(this.AvailableSlots));
        }

        private async Task LoadInterviewSessionsAsync()
        {
            this.InterviewSessions = new ObservableCollection<InterviewSession>();
            try
            {
                var sessions = await this.interviewSessionService.GetScheduledSessionsAsync();

                foreach (var session in sessions)
                {
                    this.InterviewSessions.Add(session);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sessions: {ex.Message}");
            }
        }

        private void LoadAvailableSlots()
        {
            // this doesnt actually like... do anything? from what i can see?
            this.MatchedCompanies =
            [
                new CompanyPosting { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new CompanyPosting { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 }
            ];
        }

        private async Task ScheduleInterview(CompanyPosting company)
        {
            this.IsBookingVisible = true;
            this.SelectedCompany = company;

            var slots = await this.bookingService.GetAvailableSlotsByRecruiterId(company.RecruiterId);

            var slotsGroupedByDay = slots.GroupBy(slot => slot.StartTime.Date);
            var firstSlotPerDay = slotsGroupedByDay.Select(group => group.First());
            this.AvailableDays = firstSlotPerDay.ToList();

            this.SelectedDay = this.AvailableDays.FirstOrDefault()?.StartTime.Date ?? DateTime.Today;
        }

        private async Task LoadSlotsForSelectedDay()
        {
            if (this.SelectedCompany == null)
            {
                return;
            }

            var allSlots = await this.bookingService.GetAvailableSlotsByRecruiterId(this.SelectedCompany.RecruiterId);
            this.AvailableSlots = allSlots
                .Where(slot => slot.StartTime.Date == this.SelectedDay.Date)
                .ToList();
        }

        private async Task ConfirmInterviewCommandExecute(object? obj)
        {
            if (this.SelectedSlot == null)
            {
                return;
            }

            if (this.SelectedCompany == null)
            {
                return;
            }

            await this.bookingService.ConfirmBooking(Env.USER_ID, this.SelectedSlot);
            await this.LoadInterviewSessionsAsync();

            try
            {
                this.notificationService.ShowBookingConfirmed(
                    this.SelectedCompany.CompanyName,
                    this.SelectedCompany.JobTitle,
                    this.SelectedSlot.StartTime,
                    this.SelectedSlot.EndTime);
            }
            catch
            {
            }

            this.MatchedCompanies.Remove(this.SelectedCompany);
            this.IsBookingVisible = false;
        }

        [ExcludeFromCodeCoverage]
        private void JoinInterview(object obj)
        {
            try
            {
                var interviewSession = (InterviewSession)obj;
                var interviewPage = new InterviewCandidatePage(interviewSession);
                var interviewWindow = new Window();

                interviewPage.Tag = interviewWindow;
                interviewPage.OnClosed = this.OnInterviewWindowClosed;

                interviewWindow.Content = interviewPage;
                interviewWindow.Activate();
            }
            catch
            {
            }
        }

        [ExcludeFromCodeCoverage]
        private void OnInterviewWindowClosed()
        {
            this.LoadInterviewSessionsAsync().ConfigureAwait(false);
        }

        private async Task CancelInterviewCommandExecute(object? obj)
        {
            if (obj is not InterviewSession session)
            {
                return;
            }

            try
            {
                var connectedInterviewSession = await this.interviewSessionService.GetSessionAsync(session.Id);
                if (connectedInterviewSession != null)
                {
                    await this.interviewSessionService.DeleteSessionAsync(connectedInterviewSession.Id);
                }

                await this.LoadInterviewSessionsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cancellation failed: {ex.Message}");
            }
        }

        private async Task LoadAvailableSlotsCommandExecute(object? obj)
        {
            this.LoadAvailableSlots();
        }

        private async Task ScheduleInterviewCommandExecute(object? obj)
        {
            if (obj is not CompanyPosting company)
            {
                return;
            }

            await this.ScheduleInterview(company);
        }

        private void JoinInterviewCommandExecute(object? obj)
        {
            if (obj == null)
            {
                return;
            }

            this.JoinInterview(obj);
        }

        private void SelectDayForInterviewCommandExecute(object? obj)
        {
            if (obj is not Slot selectedSlot)
            {
                return;
            }

            foreach (var day in this.AvailableDays)
            {
                day.IsDaySelected = false;
            }

            selectedSlot.IsDaySelected = true;
            this.SelectedDay = selectedSlot.StartTime.Date;
            this.SelectedSlot = null;
            this.OnPropertyChanged(nameof(this.AvailableDays));
        }

        private void LoadNextDaysCommandExecute(object? obj)
        {
            if (this.dayStartIndex + VISIBLEDAYSCOUNT < (this.AvailableDays?.Count ?? 0))
            {
                this.dayStartIndex++;
                this.OnPropertyChanged(nameof(this.VisibleDays));
            }
        }

        private void LoadPreviousDaysCommandExecute(object? obj)
        {
            if (this.dayStartIndex > 0)
            {
                this.dayStartIndex--;
                this.OnPropertyChanged(nameof(this.VisibleDays));
            }
        }

        private ObservableCollection<CompanyPosting> GetMatchedCompanies()
        {
            // This is just a placeholder, this data will come from another team
            return new ObservableCollection<CompanyPosting>
            {
                new CompanyPosting { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new CompanyPosting { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 },
            };
        }
    }
}