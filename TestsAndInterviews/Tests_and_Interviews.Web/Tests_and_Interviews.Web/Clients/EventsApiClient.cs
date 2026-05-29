namespace Tests_and_Interviews.Web.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Net.WebSockets;
    using Tests_and_Interviews.Web.Dtos;
    public class EventsApiClient
    {
        private readonly HttpClient _http;
        private static string s_apiPath = "api/events";

        public EventsApiClient(HttpClient http)
        {
            this._http = http;
        }

        public async Task<List<EventDto>> GetCurrentEvents(int userId)
        {
            var response = await this._http.GetAsync($"{s_apiPath}/current/{userId}");
            if (!response.IsSuccessStatusCode)
                return new List<EventDto>();
            return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new List<EventDto>();
        }

        public async Task<List<EventDto>> GetPastEvents(int userId)
        {
            var response = await this._http.GetAsync($"{s_apiPath}/past/{userId}");
            if (!response.IsSuccessStatusCode)
                return new List<EventDto>();
            return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new List<EventDto>();
        }

        public async Task<EventDto?> Create(EventDto dto)
        {
            var response = await this._http.PostAsJsonAsync(s_apiPath, dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>();
        }

        public async Task Update (int id, EventDto dto)
        {
            var response = await this._http.PutAsJsonAsync($"{s_apiPath}/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task Delete(int id)
        {
            var response = await this._http.DeleteAsync($"{s_apiPath}/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<CollaboratorDto>> GetCollaborators(int companyId)
        {
            var response = await this._http.GetAsync($"api/collaborators/{companyId}");
            if (!response.IsSuccessStatusCode)
                return new List<CollaboratorDto>();
            return await response.Content.ReadFromJsonAsync<List<CollaboratorDto>>() ?? new List<CollaboratorDto>();
        }

        public async Task<List<EventDto>> GetAllCurrentEvents()
        {
            var response = await this._http.GetAsync($"{s_apiPath}/current");
            if (!response.IsSuccessStatusCode)
            {
                return new List<EventDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new List<EventDto>();
        }

        public async Task<List<EventDto>> GetAllPastEvents()
        {
            var response = await this._http.GetAsync($"{s_apiPath}/past");
            if (!response.IsSuccessStatusCode)
            {
                return new List<EventDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new List<EventDto>();
        }
    }
}
