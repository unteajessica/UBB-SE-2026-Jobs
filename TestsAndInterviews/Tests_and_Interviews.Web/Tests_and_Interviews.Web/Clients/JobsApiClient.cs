namespace Tests_and_Interviews.Web.Clients
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Dtos;

    /// <summary>
    /// Proxy client that calls the Jobs API endpoints on behalf of the MVC web app.
    /// </summary>
    public class JobsApiClient
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobsApiClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client configured with the API base address.</param>
        public JobsApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Attaches the JWT token from the cookie session to outgoing requests.
        /// </summary>
        /// <param name="jwt">The JWT token string stored in the user's claims.</param>
        public void SetAuthToken(string jwt)
        {
            this.httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);
        }

        /// <summary>
        /// Retrieves all job postings from the API.
        /// </summary>
        /// <returns>A list of all job postings.</returns>
        public async Task<List<JobPostingDto>> GetAllJobsAsync()
        {
            List<JobPostingDto>? result =
                await this.httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/jobs");
            return result ?? new List<JobPostingDto>();
        }

        /// <summary>
        /// Retrieves a single job posting by its ID.
        /// </summary>
        /// <param name="jobId">The job posting ID.</param>
        /// <returns>The matching job posting, or null if not found.</returns>
        public async Task<JobPostingDto?> GetJobByIdAsync(int jobId)
        {
            List<JobPostingDto> jobs = await this.GetAllJobsAsync();
            return jobs.Find(j => j.JobId == jobId);
        }

        /// <summary>
        /// Retrieves all available skills from the API.
        /// </summary>
        /// <returns>A list of skills.</returns>
        public async Task<List<SkillDto>> GetAllSkillsAsync()
        {
            List<SkillDto>? result =
                await this.httpClient.GetFromJsonAsync<List<SkillDto>>("api/jobs/skills");
            return result ?? new List<SkillDto>();
        }

        /// <summary>
        /// Creates a new job posting via the API.
        /// </summary>
        /// <param name="dto">The data for the new job posting.</param>
        /// <returns>True if creation succeeded.</returns>
        public async Task<bool> AddJobAsync(AddJobDto dto)
        {
            Debug.WriteLine(dto);
            Debug.WriteLine($"Authorization header: {this.httpClient.DefaultRequestHeaders.Authorization}");
            HttpResponseMessage response =
                await this.httpClient.PostAsJsonAsync("api/jobs", dto);

            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Response Status: {response.StatusCode}");
                Debug.WriteLine($"API Response Content: {content}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Updates an existing job posting via the API.
        /// </summary>
        /// <param name="jobId">The ID of the job to update.</param>
        /// <param name="dto">The updated job data.</param>
        /// <returns>True if update succeeded.</returns>
        public async Task<bool> UpdateJobAsync(int jobId, JobPostingDto dto)
        {
            HttpResponseMessage response =
                await this.httpClient.PutAsJsonAsync($"api/jobs/{jobId}", dto);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a job posting via the API.
        /// </summary>
        /// <param name="jobId">The ID of the job to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        public async Task<bool> DeleteJobAsync(int jobId)
        {
            HttpResponseMessage response =
                await this.httpClient.DeleteAsync($"api/jobs/{jobId}");
            return response.IsSuccessStatusCode;
        }
    }
}