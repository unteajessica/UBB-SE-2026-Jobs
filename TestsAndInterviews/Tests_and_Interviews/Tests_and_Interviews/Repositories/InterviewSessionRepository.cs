namespace Tests_and_Interviews.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Repositories.Interfaces;

    /// <summary>
    /// Repository for managing interview sessions.
    /// </summary>
    public class InterviewSessionRepository : IInterviewSessionRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewSessionRepository"/> class.
        /// </summary>
        public InterviewSessionRepository()
        {
            this.appDbContext = new AppDbContext();
        }

        /// <inheritdoc/>
        public async Task<InterviewSession> GetInterviewSessionByIdAsync(int id)
        {
            var session = await this.appDbContext.InterviewSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
            {
                throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
            }

            return session;
        }

        /// <inheritdoc/>
        public InterviewSession GetInterviewSessionById(int id)
        {
            var session = this.appDbContext.InterviewSessions
                .FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
            }

            return session;
        }

        /// <inheritdoc/>
        public async Task UpdateInterviewSessionAsync(InterviewSession updated)
        {
            var existing = await this.appDbContext.InterviewSessions
                .FirstOrDefaultAsync(s => s.Id == updated.Id);

            if (existing == null)
            {
                return;
            }

            existing.InterviewerId = updated.InterviewerId;
            existing.PositionId = updated.PositionId;
            existing.ExternalUserId = updated.ExternalUserId;
            existing.Status = updated.Status;
            existing.DateStart = updated.DateStart;
            existing.Video = updated.Video;
            existing.Score = updated.Score;

            await this.appDbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public void Add(InterviewSession session)
        {
            this.appDbContext.InterviewSessions.Add(session);
            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void Delete(InterviewSession session)
        {
            this.appDbContext.InterviewSessions.Remove(session);
            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task<List<InterviewSession>> GetScheduledSessionsAsync()
        {
            return await this.appDbContext.InterviewSessions
                .Where(s => s.Status == InterviewStatus.Scheduled.ToString())
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<InterviewSession>> GetSessionsByStatusAsync(string status)
        {
            return await this.appDbContext.InterviewSessions
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.DateStart)
                .ToListAsync();
        }
    }
}