namespace PussyCats.Web.Clients
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using PussyCats.Web.Dtos;

    /// <summary>
    /// Proxy client that calls the Applicants API endpoints on behalf of the MVC web app.
    /// </summary>
    public class ApplicantsApiClient
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicantsApiClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client configured with the API base address.</param>
        public ApplicantsApiClient(HttpClient httpClient)
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
        /// Creates a new applicant application via the API.
        /// </summary>
        /// <param name="dto">The applicant application data.</param>
        /// <returns>The created applicant, or null if creation failed.</returns>
        public async Task<ApplicantDto?> CreateApplicantAsync(ApplicantDto dto)
        {
            try
            {
                Debug.WriteLine($"Creating applicant for JobId: {dto.JobId}, UserId: {dto.UserId}");
                Debug.WriteLine($"Authorization header: {this.httpClient.DefaultRequestHeaders.Authorization}");

                HttpResponseMessage response =
                    await this.httpClient.PostAsJsonAsync("api/applicants", dto);

                if (response.IsSuccessStatusCode)
                {
                    ApplicantDto? result = await response.Content.ReadFromJsonAsync<ApplicantDto>();
                    Debug.WriteLine($"Successfully created applicant: {result?.ApplicantId}");
                    return result;
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API Response Status: {response.StatusCode}");
                    Debug.WriteLine($"API Response Content: {content}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception creating applicant: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves all applicants for a specific job.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A list of applicants for the job.</returns>
        public async Task<List<ApplicantDto>> GetApplicantsByJobAsync(int jobId)
        {
            try
            {
                HttpResponseMessage response = 
                    await this.httpClient.GetAsync($"api/applicants/byjob/{jobId}");

                if (response.IsSuccessStatusCode)
                {
                    List<ApplicantDto>? result = 
                        await response.Content.ReadFromJsonAsync<List<ApplicantDto>>();
                    return result ?? new List<ApplicantDto>();
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Debug.WriteLine($"No applicants found for job {jobId} (404)");
                    return new List<ApplicantDto>();
                }
                else
                {
                    Debug.WriteLine($"Error fetching applicants for job {jobId}: {response.StatusCode}");
                    return new List<ApplicantDto>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception fetching applicants for job {jobId}: {ex.Message}");
                return new List<ApplicantDto>();
            }
        }

        /// <summary>
        /// Retrieves an applicant by their ID.
        /// </summary>
        /// <param name="applicantId">The applicant ID.</param>
        /// <returns>The applicant, or null if not found.</returns>
        public async Task<ApplicantDto?> GetApplicantByIdAsync(int applicantId)
        {
            try
            {
                ApplicantDto? result =
                    await this.httpClient.GetFromJsonAsync<ApplicantDto>($"api/applicants/{applicantId}");
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a user has already applied for a specific job.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if the user has already applied, false otherwise.</returns>
        public async Task<bool> HasUserAppliedAsync(int jobId, int userId)
        {
            List<ApplicantDto> applicants = await this.GetApplicantsByJobAsync(jobId);
            return applicants.Any(a => a.UserId == userId);
        }
    }
}

