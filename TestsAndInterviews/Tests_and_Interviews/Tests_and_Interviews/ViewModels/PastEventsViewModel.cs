using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.ViewModels
{
    public partial class PastEventsViewModel : ObservableObject
    {
        private readonly IEventsService eventsService;
        private readonly SessionService sessionService;

        public ObservableCollection<Event> PastEventsCollection { get; }

        /// <summary>
        /// Past Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public PastEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            this.eventsService = eventsService;
            this.sessionService = sessionService;

            PastEventsCollection = new ObservableCollection<Event>();
        }

        public async Task LoadPastEventsAsync()
        {
            PastEventsCollection.Clear();
            var events = await this.eventsService.GetPastEvents(this.sessionService.LoggedInUser.CompanyId);
            foreach (var @event in events)
            {
                PastEventsCollection.Add(@event);
            }
        }
    }
}