namespace Tests_and_Interviews.Web.Clients
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Web.Dtos;

    /// <summary>
    /// Client responsible for communicating with payment endpoints.
    /// </summary>
    public class PaymentApiClient
    {
        private readonly HttpClient http;
        private static readonly string ApiPath = "api/payment";

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentApiClient"/> class.
        /// </summary>
        /// <param name="http">The HTTP client.</param>
        public PaymentApiClient(HttpClient http)
        {
            this.http = http;
        }

        /// <summary>
        /// Processes a payment for a job promotion.
        /// </summary>
        public async Task<string> ProcessPaymentAsync(
            int jobId,
            int amount,
            string cardHolderName,
            string cardNumber,
            string expDate,
            string cvv)
        {
            var payload = new
            {
                CardHolderName = cardHolderName,
                CardNumber = cardNumber,
                ExpDate = expDate,
                Cvv = cvv
            };

            HttpResponseMessage response =
                await this.http.PostAsJsonAsync(
                    $"{ApiPath}/process/{jobId}?paymentAmount={amount}",
                    payload);

            if (!response.IsSuccessStatusCode)
            {
                return "Payment failed. Please try again.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves paid jobs filtered by job type and experience level.
        /// </summary>
        public async Task<List<JobPaymentInfoDto>> GetPaidJobsInfoAsync(
            string jobType,
            string experienceLevel)
        {
            var response = await this.http.GetAsync(
                $"{ApiPath}/paid?jobType={jobType}&experienceLevel={experienceLevel}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<JobPaymentInfoDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<JobPaymentInfoDto>>()
                ?? new List<JobPaymentInfoDto>();
        }
    }
}