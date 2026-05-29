namespace Tests_and_Interviews.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Collaborator class represents the association between an event and a company that is collaborating on that event. It contains properties for the event ID and company ID, as well as navigation properties to the related Event and Company entities.
    /// </summary>
    [Table("collaborators")]
    public class Collaborator
    {
        /// <summary>
        /// Gets or sets the unique identifier for the event associated with the collaboration. 
        /// This property is mapped to the "event_id" column in the database and represents a foreign key relationship to the Event entity.
        /// </summary>
        [Column("event_id")]
        public int EventId { get; set; }

        /// <summary>
        /// Gets or sets the event associated with the collaboration. This property represents a navigation property to the Event entity, allowing for access to the event's details and related information.
        /// </summary>
        public Event Event { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier for the company associated with the collaboration.
        /// </summary>
        [Column("company_id")]
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the company associated with the collaboration. This property represents a navigation property to the Company entity, allowing for access to the company's details and related information.
        /// </summary>
        public Company Company { get; set; } = null!;
    }
}