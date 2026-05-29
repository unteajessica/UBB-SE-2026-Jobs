namespace PussyCats.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PussyCats.Web.Clients;
    using PussyCats.Web.Dtos;

    /// <summary>
    /// Handles payment-related pages and actions.
    /// </summary>
    [Authorize(Roles = "Recruiter")]
    public class PaymentController : Controller
    {
        private readonly PaymentApiClient paymentApiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentController"/> class.
        /// </summary>
        /// <param name="paymentApiClient">The payment API client.</param>
        public PaymentController(PaymentApiClient paymentApiClient)
        {
            this.paymentApiClient = paymentApiClient;
        }

        /// <summary>
        /// Displays the payment form.
        /// </summary>
        /// <param name="jobId">The ID of the job being promoted.</param>
        public async Task<IActionResult> Index(int jobId)
        {
            this.TempData.Remove("SuccessMessage");
            this.TempData.Remove("ErrorMessage");

            List<JobPaymentInfoDto> paymentData =
                await this.paymentApiClient.GetPaidJobsInfoAsync(
                    "Internship",
                    "Entry level");

            this.ViewBag.JobId = jobId;
            this.ViewBag.PaymentData = paymentData;

            return this.View();
        }

        /// <summary>
        /// Handles payment submission.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="cardHolderName">The card holder name.</param>
        /// <param name="cardNumber">The card number.</param>
        /// <param name="expDate">The expiration date.</param>
        /// <param name="cvv">The CVV code.</param>
        /// <param name="amount">The payment amount.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPayment(
            int jobId,
            string cardHolderName,
            string cardNumber,
            string expDate,
            string cvv,
            int amount)
        {
            if (amount <= 0)
            {
                this.TempData["ErrorMessage"] =
                    "Please enter a valid amount greater than 0.";

                return this.RedirectToAction(nameof(this.Index), new { jobId });
            }

            string result = await this.paymentApiClient.ProcessPaymentAsync(
                jobId,
                amount,
                cardHolderName,
                cardNumber,
                expDate,
                cvv);

            if (!string.IsNullOrEmpty(result))
            {
                this.TempData["ErrorMessage"] = result;
                return this.RedirectToAction(
                    nameof(this.PaymentFailure));
            }

            this.TempData["SuccessMessage"] =
                "Payment processed successfully!";

            return this.RedirectToAction(
                nameof(this.PaymentSuccess));
        }

        /// <summary>
        /// Displays payment success page.
        /// </summary>
        public IActionResult PaymentSuccess()
        {
            return this.View();
        }

        /// <summary>
        /// Displays payment failure page.
        /// </summary>
        public IActionResult PaymentFailure()
        {
            return this.View();
        }
    }
}
