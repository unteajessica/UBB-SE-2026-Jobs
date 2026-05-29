using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IEventsService
    {
        Task<Event> AddEvent(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int hostId, List<Company> collaborators);
        Task DeleteEvent(Event eventToBeRemoved);
        Task<ObservableCollection<Event>> GetCurrentEvents(int loggedInUserID);
        Task<ObservableCollection<Event>> GetPastEvents(int loggedInUserID);
        Task UpdateEvent(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation);
    }
}