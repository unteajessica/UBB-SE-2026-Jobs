namespace Tests_and_Interviews.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Repositories.Interfaces;

    /// <inheritdoc cref="ILeaderboardRepository"/>
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardRepository"/> class.
        /// </summary>
        public LeaderboardRepository()
        {
            this.appDbContext = new AppDbContext();
        }

        /// <inheritdoc />
        public async Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId)
        {
            return await this.appDbContext.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .Where(le => le.TestId == testId)
                .OrderBy(le => le.RankPosition)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit)
        {
            return await this.appDbContext.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .Where(le => le.TestId == testId)
                .OrderBy(le => le.RankPosition)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId)
        {
            return await this.appDbContext.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .FirstOrDefaultAsync(le => le.UserId == userId && le.TestId == testId);
        }

        /// <inheritdoc />
        public async Task DeleteByTestIdAsync(int testId)
        {
            var entries = await this.appDbContext.LeaderboardEntries
                .Where(le => le.TestId == testId)
                .ToListAsync();

            this.appDbContext.LeaderboardEntries.RemoveRange(entries);
            await this.appDbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task SaveRangeAsync(List<LeaderboardEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            await this.appDbContext.LeaderboardEntries.AddRangeAsync(entries);
            await this.appDbContext.SaveChangesAsync();
        }
    }
}