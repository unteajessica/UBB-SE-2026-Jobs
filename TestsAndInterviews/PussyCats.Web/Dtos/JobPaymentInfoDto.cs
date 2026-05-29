namespace PussyCats.Web.Dtos
{
    /// <summary>
    /// Represents payment information for a job listing.
    /// </summary>
    public class JobPaymentInfoDto
    {
        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public int AmountPayed { get; set; }
    }
}
