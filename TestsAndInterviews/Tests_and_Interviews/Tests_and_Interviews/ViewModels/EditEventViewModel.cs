using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.ViewModels
{
    public partial class EditEventViewModel : ObservableObject
    {
        private const string EmptyStringValue = "";
        private const string ErrorInputsInvalid = "Please enter valid inputs before creating an event";

        private readonly IEventsService eventsService;
        private readonly Event eventToEdit;
        private readonly IEventValidator eventValidator;

        [ObservableProperty] private string photo = EmptyStringValue;

        [ObservableProperty] private string title = EmptyStringValue;
        [ObservableProperty] private string titleError = EmptyStringValue;
        private bool titleIsValid = true;

        [ObservableProperty] private string description = EmptyStringValue;
        [ObservableProperty] private string descriptionError = EmptyStringValue;
        private bool descriptionIsValid = true;

        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private string startDateError = EmptyStringValue;
        private bool startDateIsValid = true;

        [ObservableProperty] private DateTimeOffset? endDate;
        [ObservableProperty] private string endDateError = EmptyStringValue;
        private bool endDateIsValid = true;

        [ObservableProperty] private string location = EmptyStringValue;
        [ObservableProperty] private string locationError = EmptyStringValue;
        private bool locationIsValid = true;

        [ObservableProperty] private string addError = EmptyStringValue;

        public bool IsEverythingValid => (AddError == EmptyStringValue);
        public bool EventUpdatedSuccessfully = false;
        public bool EventDeletedSuccessfully = false;

        /// <summary>
        /// Edit Event View Model constructor which sets the textboxes' values to the event's
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="selectedEvent"> the selected event to be modified </param>
        /// <param name="eventValidator"> event validator service </param>
        public EditEventViewModel(IEventsService eventsService, Event selectedEvent, IEventValidator eventValidator)
        {
            this.eventsService = eventsService;
            eventToEdit = selectedEvent;
            this.eventValidator = eventValidator;

            title = selectedEvent.Title;
            description = selectedEvent.Description;
            startDate = selectedEvent.StartDate;
            endDate = selectedEvent.EndDate;
            location = selectedEvent.Location;
        }

        /// <summary>
        /// Function that tries to update an event
        /// </summary>
        [RelayCommand]
        public void EditEvent()
        {
            if (!titleIsValid || !descriptionIsValid || !startDateIsValid || !endDateIsValid || !locationIsValid)
            {
                AddError = ErrorInputsInvalid;
                return;
            }

            try
            {
                AddError = EmptyStringValue;
                DateTime eventStartDateTime = StartDate.Value.DateTime;
                DateTime eventEndDateTime = EndDate.Value.DateTime;

                eventsService.UpdateEvent(eventToEdit.Id, Photo, Title, Description, eventStartDateTime, eventEndDateTime, Location);
                EventUpdatedSuccessfully = true;
            }
            catch (Exception)
            {
                EventUpdatedSuccessfully = false;
            }
        }

        /// <summary>
        /// Function that tries to delete an event
        /// </summary>
        [RelayCommand]
        public void DeleteEvent()
        {
            try
            {
                eventsService.DeleteEvent(eventToEdit);
                EventDeletedSuccessfully = true;
            }
            catch (Exception)
            {
                EventDeletedSuccessfully = false;
            }
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event title is valid
        /// </summary>
        /// <returns> true if the title is valid, false otherwise </returns>
        public bool ValidateTitle()
        {
            try
            {
                if (eventValidator.ValidateEventTitle(Title))
                {
                    TitleError = EmptyStringValue;
                    titleIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                TitleError = exception.Message;
                titleIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event description is valid
        /// </summary>
        /// <returns> true if the description is valid, false otherwise </returns>
        public bool ValidateDescription()
        {
            try
            {
                if (eventValidator.ValidateEventDescription(Description))
                {
                    DescriptionError = EmptyStringValue;
                    descriptionIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                DescriptionError = exception.Message;
                descriptionIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event dates are cronologically valid
        /// </summary>
        /// <returns> true if the dates are in cronological order, false otherwise </returns>
        public bool ValidateDatesCronologity()
        {
            try
            {
                if (eventValidator.ValidateEventDatesChronologically(StartDate, EndDate))
                {
                    StartDateError = EmptyStringValue;
                    EndDateError = EmptyStringValue;
                    endDateIsValid = true;
                    startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                StartDateError = exception.Message;
                EndDateError = exception.Message;
                endDateIsValid = false;
                startDateIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event location is valid
        /// </summary>
        /// <returns> true if the location is valid, false otherwise </returns>
        public bool ValidateLocation()
        {
            try
            {
                if (eventValidator.ValidateEventLocation(Location))
                {
                    LocationError = EmptyStringValue;
                    locationIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                LocationError = exception.Message;
                locationIsValid = false;
            }
            return false;
        }
    }
}