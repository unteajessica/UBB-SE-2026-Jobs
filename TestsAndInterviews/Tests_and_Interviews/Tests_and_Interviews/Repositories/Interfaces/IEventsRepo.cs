namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models;

    public interface IEventsRepo
    {
        void AddEventToRepo(Event e);
        void RemoveEventFromRepo(Event e);
        ObservableCollection<Event> GetCurrentEventsFromRepo(int loggedInUser);
        ObservableCollection<Event> GetPastEventsFromRepo(int loggedInUser);
        void UpdateEventToRepo(int id, string photo, string title, string description, DateTime start, DateTime end, string location);
    }
}
