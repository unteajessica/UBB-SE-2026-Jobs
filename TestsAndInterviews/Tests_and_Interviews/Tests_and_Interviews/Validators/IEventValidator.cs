using System;

namespace Tests_and_Interviews.Validators
{
    public interface IEventValidator
    {
        bool ValidateEventTitle(string eventTitle);
        bool ValidateEventDescription(string eventDescription);
        bool ValidateEventLocation(string eventLocation);
        bool ValidateEventStartDate(DateTimeOffset? eventStartDate);
        bool ValidateEventEndDate(DateTimeOffset? eventEndDate);
        bool ValidateEventDatesChronologically(DateTimeOffset? eventStartDate, DateTimeOffset? eventEndDate);
    }
}