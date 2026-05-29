namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// Represents the view model for recruiters, providing properties and methods to manage interview slots and pending
    /// reviews.
    /// </summary>
    /// <remarks>Implements INotifyPropertyChanged to support data binding in UI frameworks such as WPF or Xamarin.
    /// Handles asynchronous loading and updating of recruiter slots and interview sessions.</remarks>
    public class RecruiterViewModel : INotifyPropertyChanged
    {
        private readonly ISlotService slotService;
        private readonly IInterviewSessionService sessionService;

        private int currentRecruiterId = Env.RECRUITER_ID;
        private ObservableCollection<SlotDto> slots = [];
        private DateTime selectedDate = DateTime.Today;
        private ObservableCollection<InterviewSession> pendingReviews = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="RecruiterViewModel"/> class.
        /// </summary>
        /// <param name="slotService">The service used to manage interview slots.</param>
        /// <param name="sessionService">The repository used to manage interview sessions.</param>
        public RecruiterViewModel(ISlotService slotService, IInterviewSessionService sessionService)
        {
            this.slotService = slotService;
            this.sessionService = sessionService;
            _ = this.InitializeDataAsync();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecruiterViewModel"/> class.
        /// </summary>
        /// <param name="slotService">The service used to manage interview slots.</param>
        [ExcludeFromCodeCoverage]
        public RecruiterViewModel(ISlotService slotService)
        {
            this.slotService = slotService;
            this.sessionService = new InterviewSessionService();

            _ = this.InitializeDataAsync();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the currently selected date.
        /// </summary>
        /// <remarks>Raises property change notifications and triggers asynchronous slot loading when set.</remarks>
        public DateTime SelectedDate
        {
            get => this.selectedDate;
            set
            {
                if (this.selectedDate != value)
                {
                    this.selectedDate = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.SelectedDateFormatted));

                    _ = this.LoadSlotsAsync();
                }
            }
        }

        /// <summary>
        /// Gets the selected date formatted as 'dddd dd/MM/yyyy'.
        /// </summary>
        public string SelectedDateFormatted =>
            this.SelectedDate.ToString("dddd dd/MM/yyyy");

        /// <summary>
        /// Gets or sets the collection of slot data transfer objects.
        /// </summary>
        public ObservableCollection<SlotDto> Slots
        {
            get => this.slots;
            set
            {
                this.slots = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<InterviewSession> PendingReviews
        {
            get => this.pendingReviews;
            set
            {
                this.pendingReviews = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Asynchronously loads slot and pending review data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task InitializeDataAsync()
        {
            await this.LoadSlotsAsync();
            await this.LoadPendingReviewsAsync();
        }

        public async Task LoadPendingReviewsAsync()
        {
            try
            {
                var list = await this.sessionService.GetSessionsByStatusAsync(InterviewStatus.InProgress.ToString());
                this.PendingReviews = new ObservableCollection<InterviewSession>(list);
            }
            catch
            {
                this.PendingReviews = [];
            }
        }

        public void LoadPendingReviews()
        {
            this.LoadPendingReviewsAsync();
        }

        /// <summary>
        /// Asynchronously loads the slots visible to the current recruiter for the selected date and updates the Slots
        /// collection.
        /// </summary>
        /// <returns>A task that represents the asynchronous load operation.</returns>
        public async Task LoadSlotsAsync()
        {
            List<SlotDto> recruiterSlots = await this.slotService
                .LoadRecruiterVisibleSlotsAsync(
                    this.currentRecruiterId, this.SelectedDate.Date);

            this.Slots = new ObservableCollection<SlotDto>(recruiterSlots);
        }

        /// <summary>
        /// Asynchronously creates a new slot with the specified details and duration, then reloads the slot list.
        /// </summary>
        /// <param name="baseSlot">The slot details to create.</param>
        /// <param name="duration">The duration of the slot in minutes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CreateSlotAsync(SlotDto baseSlot, int duration)
        {
            await this.slotService.CreateRecruiterSlotAsync(baseSlot, duration);
            await this.LoadSlotsAsync();
        }

        /// <summary>
        /// Deletes a recruiter slot by its identifier and reloads the slot list asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the slot to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteSlotAsync(int id)
        {
            await this.slotService.DeleteRecruiterSlotAsync(id);
            await this.LoadSlotsAsync();
        }

        /// <summary>
        /// Updates an existing slot with a new start time and duration asynchronously.
        /// </summary>
        /// <param name="initialSlot">The slot to update.</param>
        /// <param name="newStartTime">The new start time to apply to the slot.</param>
        /// <param name="newDuration">The new duration, in minutes, for the slot.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateSlotAsync(SlotDto initialSlot, TimeSpan newStartTime, int newDuration)
        {
            DateTime startTime = (DateTime)(initialSlot.StartTime.Date + newStartTime);

            await this.slotService.UpdateRecruiterSlotAsync(initialSlot, startTime, newDuration);
            await this.LoadSlotsAsync();
        }

        /// <summary>
        /// Raises the PropertyChanged event to notify listeners of a property value change.
        /// </summary>
        /// <param name="name">The name of the property that changed. This value is optional and can be supplied automatically.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}