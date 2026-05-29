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
    public partial class OurEventsViewModel : ObservableObject
    {
        private readonly IEventsService eventsService;
        private readonly SessionService sessionService;

        [ObservableProperty]
        private ObservableCollection<Event> currentEventsCollection = new();

        /// <summary>
        /// Our Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public OurEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            this.eventsService = eventsService;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Loads the current events asynchronously.
        /// </summary>
        public async Task LoadEventsAsync()
        {
            try
            {
                var events = await this.eventsService.GetCurrentEvents(this.sessionService.LoggedInUser.CompanyId);
                this.CurrentEventsCollection.Clear();
                foreach (var eventItem in events)
                {
                    this.CurrentEventsCollection.Add(eventItem);
                }
            }
            catch
            {
                // Handle error silently for now
            }
        }
    }
}
