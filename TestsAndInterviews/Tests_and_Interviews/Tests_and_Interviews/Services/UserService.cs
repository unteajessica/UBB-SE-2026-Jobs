using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Api;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Mappers;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services.Interfaces;

namespace Tests_and_Interviews.Services
{
    public class UserService: IUserService
    {
        private readonly HttpClient http;

        public UserService()
        {
            this.http = ApiClient.Http;
        }

        public UserService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        public async Task<List<User>> GetAllAsync()
        {
            HttpResponseMessage response = await this.http.GetAsync($"users");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<User>();
            }

            response.EnsureSuccessStatusCode();
            List<UserDto>? usersDto = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return usersDto!.Select(user => user.ToEntity()).ToList();
        }
    }
}
