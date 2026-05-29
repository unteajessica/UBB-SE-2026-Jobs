namespace PussyCats.Web.Dtos
{
    /// <summary>
    /// Represents a company for display purposes.
    /// </summary>
    public class CompanyDto
    {
        /// <summary>
        /// Gets or sets the company ID.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}

