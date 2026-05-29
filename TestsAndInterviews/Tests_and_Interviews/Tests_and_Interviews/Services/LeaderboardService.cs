// <copyright file="LeaderboardService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;

    /// <inheritdoc cref="ILeaderboardService"/>
    public class LeaderboardService : ILeaderboardService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardService"/> class.
        /// </summary>
        public LeaderboardService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public LeaderboardService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Triggers a server-side recalculation of the leaderboard for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        public async Task RecalculateLeaderboardAsync(int testId)
        {
            HttpResponseMessage response = await this.http.PostAsync($"leaderboard/recalculate/{testId}", null);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<List<LeaderboardEntry>> GetTopThreeAsync(int testId)
        {
            await this.RecalculateLeaderboardAsync(testId);
            HttpResponseMessage response = await this.http.GetAsync($"leaderboard/bytest/{testId}/top/3");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<LeaderboardEntry>();

            response.EnsureSuccessStatusCode();
            List<LeaderboardEntryDto>? dtos = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
            return dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<LeaderboardEntry>();
        }

        /// <inheritdoc />
        public async Task<LeaderboardEntry?> GetUserRankingAsync(int userId, int testId)
        {
            await this.RecalculateLeaderboardAsync(testId);
            HttpResponseMessage response = await this.http.GetAsync($"leaderboard/bytest/{testId}/byuser/{userId}");
            if (!response.IsSuccessStatusCode)
                return null;

            LeaderboardEntryDto? dto = await response.Content.ReadFromJsonAsync<LeaderboardEntryDto>();
            return dto?.ToEntity();
        }

        /// <inheritdoc />
        public async Task<List<LeaderboardEntry>> GetFullLeaderboardAsync(int testId)
        {
            await this.RecalculateLeaderboardAsync(testId);
            HttpResponseMessage response = await this.http.GetAsync($"leaderboard/bytest/{testId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<LeaderboardEntry>();

            response.EnsureSuccessStatusCode();
            List<LeaderboardEntryDto>? dtos = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
            return dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<LeaderboardEntry>();
        }
    }
}