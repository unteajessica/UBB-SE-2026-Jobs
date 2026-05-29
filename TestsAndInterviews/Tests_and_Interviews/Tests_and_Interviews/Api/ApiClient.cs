namespace Tests_and_Interviews.Api
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Provides a preconfigured HTTP client for making requests to the application's API endpoints.
    /// </summary>
    public static class ApiClient
    {
        /// <summary>
        /// Provides a shared instance of <see cref="HttpClient"/> configured to communicate with the local API service.
        /// </summary>
        /// <remarks>The <see cref="HttpClient"/> instance uses a base address of
        /// "http://localhost:5179/api/". This instance is intended for reuse throughout the application to optimize
        /// resource usage and avoid socket exhaustion. Do not dispose of this instance.</remarks>
        public static readonly HttpClient Http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5179/api/"),
        };
    }
}
