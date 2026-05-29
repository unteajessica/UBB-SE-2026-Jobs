namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    /// <summary>
    /// Represents a recruiter entity in the system, associated with a company and responsible for managing interview slots and assigned candidates.
    /// Contains properties for the recruiter's company information, as well as methods for viewing, creating, editing, and deleting interview slots.
    /// The Recruiter class is mapped to the "Recruiters" table in the database, with a composite primary key of CompanyId and UserId.
    /// It also includes navigation properties for related entities such as Company, User, AssignedCandidates, and Slots.
    /// </summary>
    [Table("Recruiters")]
    public class Recruiter
    {
        /// <summary>
        /// Gets or sets the company identifier for the recruiter.
        /// This property is part of the composite primary key for the Recruiters table.
        /// It is mapped to the "company_id" column in the database and represents the association between the recruiter and their company.
        /// </summary>
        [Column("company_id")]
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier for the recruiter.
        /// This property is part of the composite primary key for the Recruiters table.
        /// It is mapped to the "user_id" column in the database and represents the association between the recruiter and their user account.
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the company associated with the recruiter. This property is mapped to the "name" column in the database and has a maximum length of 255 characters.
        /// It is initialized to an empty string to ensure it is never null.
        /// </summary>
        [Column("name", TypeName = "nvarchar(255)")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company associated with the recruiter.
        /// This property represents a navigation property to the Company entity, allowing for access to the company's details and related information.
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user associated with the recruiter.
        /// This property represents a navigation property to the User entity, allowing for access to the user's details and related information.
        /// </summary>
        public Tests_and_Interviews_API.Models.Core.User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of candidates assigned to the recruiter.
        /// This property represents a collection of Candidate entities that are associated with the recruiter, allowing for access to the candidates' details and related information.
        /// </summary>
        public List<Candidate> AssignedCandidates { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of interview slots managed by the recruiter.
        /// This property represents a collection of Slot entities that are associated with the recruiter, allowing for access to the details of each slot such as start time, end time,
        /// duration, status, and interview type.
        /// </summary>
        public List<Slot> Slots { get; set; } = [];

        /// <summary>
        /// Gets a list of interview slots for the specified month and year, allowing the recruiter to view their schedule for that period.
        /// </summary>
        /// <param name="month">A DateTime object representing the month and year for which to retrieve the interview slots.
        /// The day component of the DateTime is ignored, and only the month and year are used for filtering.</param>
        /// <returns>A list of Slot objects representing the slots in that month.</returns>
        public List<Slot> ViewMonthlyCalendar(DateTime month)
        {
            return [.. this.Slots.Where(s => s.StartTime.Month == month.Month && s.StartTime.Year == month.Year)];
        }

        /// <summary>
        /// Creates a new interview slot and adds it to the recruiter's list of slots. This method takes a Slot object as a parameter, which contains the details of the slot to be created,
        /// </summary>
        /// <param name="slot">The Slot object representing the interview slot to be created.</param>
        public void CreateSlot(Slot slot)
        {
            this.Slots.Add(slot);
        }

        /// <summary>
        /// Edits an existing interview slot by updating its details based on the provided Slot object. This method searches for the slot with the same Id as the updatedSlot and updates its properties
        /// </summary>
        /// <param name="updatedSlot">The Slot object containing the updated details for the interview slot.</param>
        public void EditSlot(Slot updatedSlot)
        {
            var existing = this.Slots.FirstOrDefault(s => s.Id == updatedSlot.Id);
            if (existing != null)
            {
                existing.StartTime = updatedSlot.StartTime;
                existing.EndTime = updatedSlot.EndTime;
                existing.Duration = updatedSlot.Duration;
                existing.Status = updatedSlot.Status;
                existing.InterviewType = updatedSlot.InterviewType;
            }
        }

        /// <summary>
        /// Deletes an interview slot from the recruiter's list of slots based on the provided slotId. This method searches for the slot with the specified Id and removes it from the list if found.
        /// </summary>
        /// <param name="slotId">The Id of the Slot object representing the interview slot to be deleted.</param>
        /// <returns>The Slot object that was deleted, or null if no slot with the specified Id was found.</returns>
        public Slot? DeleteSlot(int slotId)
        {
            var slot = this.Slots.FirstOrDefault(s => s.Id == slotId);
            if (slot != null)
            {
                this.Slots.Remove(slot);
            }

            return slot;
        }
    }
}