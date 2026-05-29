namespace Tests_and_Interviews.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Services.Interfaces;

    /// <inheritdoc cref="IDataProcessingService"/>
    public class DataProcessingService : IDataProcessingService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingService"/> class.
        /// </summary>
        public DataProcessingService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public DataProcessingService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <inheritdoc/>
        public async Task<bool> ProcessFinalizedAttemptAsync(int attemptId)
        {
            HttpResponseMessage response = await this.http.PostAsync(
                $"dataprocessing/finalize/{attemptId}",
                content: null);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }
    }
}