using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Validators
{
    public class EventValidator : IEventValidator
    {
        private const int MaximumTitleLength = 200;
        private const int MaximumDescriptionLength = 2000;
        private const int MaximumLocationLength = 300;

        public bool ValidateEventTitle(string eventTitle)
        {
            if (string.IsNullOrWhiteSpace(eventTitle))
            {
                throw new Exception("Title is mandatory");
            }
            if (eventTitle.Length > MaximumTitleLength)
            {
                throw new Exception("Title is too long");
            }
            return true;
        }

        public bool ValidateEventDescription(string eventDescription)
        {
            if (!string.IsNullOrEmpty(eventDescription) && eventDescription.Length > MaximumDescriptionLength)
            {
                throw new Exception("Description is too long");
            }
            return true;
        }

        public bool ValidateEventLocation(string eventLocation)
        {
            if (string.IsNullOrWhiteSpace(eventLocation))
            {
                throw new Exception("Location is mandatory");
            }
            if (eventLocation.Length > MaximumLocationLength)
            {
                throw new Exception("Location is too long");
            }
            return true;
        }

        public bool ValidateEventStartDate(DateTimeOffset? eventStartDate)
        {
            if (eventStartDate == null)
            {
                throw new Exception("Starting date is mandatory");
            }
            if (eventStartDate < DateTimeOffset.Now)
            {
                throw new Exception("Event must start after creation");
            }
            return true;
        }

        public bool ValidateEventEndDate(DateTimeOffset? eventEndDate)
        {
            if (eventEndDate == null)
            {
                throw new Exception("Ending date is mandatory");
            }
            if (eventEndDate < DateTimeOffset.Now)
            {
                throw new Exception("Event must end after creation");
            }
            return true;
        }

        public bool ValidateEventDatesChronologically(DateTimeOffset? eventStartDate, DateTimeOffset? eventEndDate)
        {
            if (eventStartDate > eventEndDate)
            {
                throw new Exception("Event must begin before ending");
            }
            return true;
        }
    }
}