namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Event class represents an event organized by a company, containing properties such as photo, title, description, start and end dates, location, host company information, and a list of collaborators.
    /// It is mapped to the "events" table in the database, with a primary key of Id that is not auto-generated. The Event class provides constructors for initializing event instances and an overridden ToString method for easy representation of event details.
    /// </summary>
    [Table("events")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("event_id")]
        public int Id { get; set; }

        [Column("photo", TypeName = "nvarchar(max)")]
        public string Photo { get; set; }

        [Column("title", TypeName = "nvarchar(200)")]
        public string Title { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]

        public string Description { get; set; }

        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Column("location", TypeName = "nvarchar(300)")]
        public string Location { get; set; }

        [Column("host_company_id")]
        public int HostCompanyId { get; set; }
        public Company HostCompany { get; set; } = null!;

        [Column("posted_at", TypeName = "datetime")]
        public DateTime PostedAt { get; set;  }
        public ICollection<Collaborator> Collaborators { get; set; } = new List<Collaborator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="eventPhoto"> event photo generated path </param>
        /// <param name="eventTitle"> event title </param>
        /// <param name="eventDescription"> event description </param>
        /// <param name="eventStartDate"> event starting date </param>
        /// <param name="eventEndDate"> event ending date </param>
        /// <param name="eventLocation"> event location </param>
        /// <param name="eventHostID"> id of the company who created the event</param>
        public Event(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int eventHostID)
        {
            this.Photo = eventPhoto;
            this.Title = eventTitle;
            this.Description = eventDescription;
            this.StartDate = eventStartDate;
            this.EndDate = eventEndDate;
            this.Location = eventLocation;
            this.HostCompanyId = eventHostID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class with default values for all properties.
        /// This constructor allows for creating an empty event instance that can be populated with specific details later on.
        /// </summary>
        public Event()
        {
        }
        /// <summary>
        /// Returns a string representation of the event, including its photo, title, description, start and end dates, location, host company ID, and collaborators. This method is overridden to provide a meaningful representation of the event's details when the ToString method is called.
        /// </summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return "Event: " + this.Photo + " " + this.Title + " " + this.Description + " " +
                this.StartDate.ToString() + " " + this.EndDate.ToString() + " " + this.Location + " " + this.HostCompanyId.ToString() +
                " " + this.Collaborators.ToString() + "\n";
        }
    }
}
