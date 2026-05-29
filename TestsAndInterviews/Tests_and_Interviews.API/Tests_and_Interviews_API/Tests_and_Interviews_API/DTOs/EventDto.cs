namespace Tests_and_Interviews_API.Dtos
{
    using System;

    /// <summary>
    /// Represents an event organized by a company.
    /// </summary>
    public class EventDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the event.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the photo associated with the event.
        /// </summary>
        public string Photo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the event.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start date of the event.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the event.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the location of the event.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the host company.
        /// </summary>
        public int HostCompanyId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the event was posted.
        /// </summary>
        public DateTime PostedAt { get; set; }
    }
}