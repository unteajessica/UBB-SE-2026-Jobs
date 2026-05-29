using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    public class FakeEventsRepo : IEventsRepo
    {
        public ObservableCollection<Event> CurrentEventsToReturn = new ObservableCollection<Event>();
        public ObservableCollection<Event> PastEventsToReturn = new ObservableCollection<Event>();

        public Event AddedEvent = null;
        public Event RemovedEvent = null;
        public int UpdatedEventId = -1;
        public string UpdatedPhoto = null;
        public string UpdatedTitle = null;
        public string UpdatedDescription = null;
        public DateTime UpdatedStartDate;
        public DateTime UpdatedEndDate;
        public string UpdatedLocation = null;

        public void AddEventToRepo(Event e)
        {
            AddedEvent = e;
        }

        public void RemoveEventFromRepo(Event e)
        {
            RemovedEvent = e;
        }

        public ObservableCollection<Event> GetCurrentEventsFromRepo(int loggedInUser)
        {
            return CurrentEventsToReturn;
        }

        public ObservableCollection<Event> GetPastEventsFromRepo(int loggedInUser)
        {
            return PastEventsToReturn;
        }

        public void UpdateEventToRepo(int id, string photo, string title, string description,
            DateTime start, DateTime end, string location)
        {
            UpdatedEventId = id;
            UpdatedPhoto = photo;
            UpdatedTitle = title;
            UpdatedDescription = description;
            UpdatedStartDate = start;
            UpdatedEndDate = end;
            UpdatedLocation = location;
        }
    }
}
