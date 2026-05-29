// <copyright file="EventsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews_API.Mappers;

    public class EventsService : IEventsService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Events service constructor
        /// </summary>
        public EventsService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public EventsService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Function that creates a new event
        /// </summary>
        /// <param name="eventPhoto"> the generated image path </param>
        /// <param name="eventTitle"> the event's title </param>
        /// <param name="eventDescription"> the event's description </param>
        /// <param name="eventStartDate"> the event's starting date </param>
        /// <param name="eventEndDate"> the event's ending date </param>
        /// <param name="eventLocation"> the event's location </param>
        /// <param name="collaborators"> a list of all the companies collaborating on the event </param>
        public async Task<Event> AddEvent(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int hostId, List<Company> collaborators)
        {
            Event eventToBeAdded = new Event(eventPhoto, eventTitle, eventDescription,
                eventStartDate, eventEndDate, eventLocation, hostId);

            HttpResponseMessage eventResponse = await this.http.PostAsJsonAsync(
                "events",
                eventToBeAdded.ToDto());
            eventResponse.EnsureSuccessStatusCode();
            EventDto? createdDto = await eventResponse.Content.ReadFromJsonAsync<EventDto>();
            Event createdEvent = createdDto!.ToEntity();

            if (collaborators != null)
            {
                foreach (var company in collaborators)
                {
                    CollaboratorDto collaboratorDto = new CollaboratorDto
                    {
                        EventId = createdEvent.Id,
                        CompanyId = company.CompanyId,
                    };
                    HttpResponseMessage collaboratorResponse = await this.http.PostAsJsonAsync(
                        $"collaborators?loggedInUserID={hostId}",
                        collaboratorDto);
                    collaboratorResponse.EnsureSuccessStatusCode();
                }   
            }

            return createdEvent;
        }

        /// <summary>
        /// Function that updates the information of an event
        /// </summary>
        /// <param name="eventIdToBeUpdated"> the id of the event that's updated </param>
        /// <param name="newEventPhoto"> the updated photo path </param>
        /// <param name="newEventTitle"> the updated title of the event </param>
        /// <param name="newEventDescription"> the updated description of the event </param>
        /// <param name="newEventStartDate"> the updated starting date of the event </param>
        /// <param name="newEventEndDate"> the updated ending date of the event </param>
        /// <param name="newEventLocation"> the updated location of the event </param>
        public async Task UpdateEvent(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation)
        {
            EventDto dto = new EventDto
            {
                Id = eventIdToBeUpdated,
                Photo = newEventPhoto,
                Title = newEventTitle,
                Description = newEventDescription,
                StartDate = newEventStartDate,
                EndDate = newEventEndDate,
                Location = newEventLocation,
            };
            HttpResponseMessage response = await this.http.PutAsJsonAsync(
                $"events/{eventIdToBeUpdated}",
                dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Function that deletes an event
        /// </summary>
        /// <param name="eventToBeRemoved"> event selected to be removed </param>
        public async Task DeleteEvent(Event eventToBeRemoved)
        {
            HttpResponseMessage response = await this.http.DeleteAsync($"events/{eventToBeRemoved.Id}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Function that returns a collection of all the current events
        /// </summary>
        /// <returns> ObservableCollection of the current events </returns>
        public async Task<ObservableCollection<Event>> GetCurrentEvents(int loggedInUserID)
        {
            HttpResponseMessage response = await this.http.GetAsync($"events/current/{loggedInUserID}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ObservableCollection<Event>(new List<Event>());
            }

            response.EnsureSuccessStatusCode();
            List<EventDto>? dtos = await response.Content.ReadFromJsonAsync<List<EventDto>>();
            return new ObservableCollection<Event>(
                dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<Event>());
        }

        /// <summary>
        /// Function that returns a collection of all the past events
        /// </summary>
        /// <returns> ObservableCollection of the past events </returns>
        public async Task<ObservableCollection<Event>> GetPastEvents(int loggedInUserID)
        {
            HttpResponseMessage response = await this.http.GetAsync($"events/past/{loggedInUserID}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ObservableCollection<Event>(new List<Event>());
            }

            response.EnsureSuccessStatusCode();
            List<EventDto>? dtos = await response.Content.ReadFromJsonAsync<List<EventDto>>();
            return new ObservableCollection<Event>(
                dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<Event>());
        }
    }
}