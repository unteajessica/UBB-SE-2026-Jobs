namespace Tests_and_Interviews.Repositories
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Repositories.Interfaces;

    public class EventsRepo : IEventsRepo
    {
        private readonly AppDbContext appDbContext;

        public EventsRepo()
        {
            this.appDbContext = new AppDbContext();
        }

        /// <inheritdoc/>
        public void AddEventToRepo(Event eventToBeAdded)
        {
            using var transaction = this.appDbContext.Database.BeginTransaction();

            try
            {
                eventToBeAdded.PostedAt = DateTime.Now;

                this.appDbContext.Events.Add(eventToBeAdded);
                this.appDbContext.SaveChanges();

                if (eventToBeAdded.Collaborators != null)
                {
                    foreach (var collaborator in eventToBeAdded.Collaborators)
                    {
                        bool alreadyCollaborates = this.appDbContext.Collaborators
                            .Any(c => c.CompanyId == collaborator.CompanyId);

                        collaborator.EventId = eventToBeAdded.Id;
                        this.appDbContext.Collaborators.Add(collaborator);
                        this.appDbContext.SaveChanges();

                        if (!alreadyCollaborates)
                        {
                            var company = this.appDbContext.Companies.Find(collaborator.CompanyId);
                            if (company != null)
                            {
                                company.CollaboratorsCount += 1;
                                this.appDbContext.SaveChanges();
                            }
                        }
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc/>
        public void RemoveEventFromRepo(Event eventToBeRemoved)
        {
            var existing = this.appDbContext.Events.Find(eventToBeRemoved.Id);
            if (existing != null)
            {
                this.appDbContext.Events.Remove(existing);
                this.appDbContext.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Event> GetCurrentEventsFromRepo(int loggedInUser)
        {
            var events = this.appDbContext.Events
                .Where(e => e.HostCompanyId == loggedInUser
                    && e.EndDate >= DateTime.Now.Date)
                .ToList();

            return new ObservableCollection<Event>(events);
        }

        /// <inheritdoc/>
        public ObservableCollection<Event> GetPastEventsFromRepo(int loggedInUser)
        {
            var events = this.appDbContext.Events
                .Where(e => e.HostCompanyId == loggedInUser
                    && e.EndDate < DateTime.Now.Date)
                .ToList();

            return new ObservableCollection<Event>(events);
        }

        /// <inheritdoc/>
        public void UpdateEventToRepo(int id, string photo, string title, string description, DateTime start, DateTime end, string location)
        {
            var existing = this.appDbContext.Events.Find(id);
            if (existing == null)
            {
                return;
            }

            existing.Photo = photo;
            existing.Title = title;
            existing.Description = description;
            existing.StartDate = start;
            existing.EndDate = end;
            existing.Location = location;
            existing.PostedAt = DateTime.Now;

            this.appDbContext.SaveChanges();
        }
    }
}