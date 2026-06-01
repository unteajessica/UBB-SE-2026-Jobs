namespace Tests_and_Interviews_API.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Models.Enums;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    /// <summary>
    /// Provides methods for managing Slot entities in the database, including retrieval, creation, update, and deletion
    /// operations for recruiter slots.
    /// </summary>
    public class SlotRepository : ISlotRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotRepository"/> class.
        /// </summary>
        public SlotRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <inheritdoc />
        public async Task<List<Slot>> GetSlotsAsync(int recruiterId, DateTime date)
        {
            return await this.appDbContext.Slots
                .Where(s => s.RecruiterId == recruiterId
                    && s.StartTime.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Slot>> GetAvailableByDateAsync(DateTime date)
        {
            return await this.appDbContext.Slots
                .Where(s => s.StartTime.Date == date.Date && s.StatusValue == 0)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Slot>> GetAllSlotsAsync(int recruiterId)
        {
            return await this.appDbContext.Slots
                .Where(s => s.RecruiterId == recruiterId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Slot?> GetByIdAsync(int id)
        {
            return await this.appDbContext.Slots
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task AddAsync(Slot slot)
        {
            bool overlaps = await this.appDbContext.Slots
                .AnyAsync(s => s.RecruiterId == slot.RecruiterId
                    && s.StartTime.Date == slot.StartTime.Date
                    && slot.StartTime < s.EndTime
                    && slot.EndTime > s.StartTime);

            if (overlaps)
            {
                throw new Exception("Slot overlaps with an existing appointment!");
            }

            this.appDbContext.Slots.Add(slot);
            await this.appDbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Slot slot)
        {
            bool overlaps = await this.appDbContext.Slots
                .AnyAsync(s => s.RecruiterId == slot.RecruiterId
                    && s.Id != slot.Id
                    && slot.StartTime < s.EndTime
                    && slot.EndTime > s.StartTime);

            if (overlaps)
            {
                throw new Exception("Slot overlaps with an existing appointment!");
            }

            var existing = await this.appDbContext.Slots.FindAsync(slot.Id);
            if (existing == null)
            {
                throw new Exception("Slot not found");
            }

            existing.StartTime = slot.StartTime;
            existing.EndTime = slot.EndTime;
            existing.Duration = slot.Duration;
            await this.appDbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int id)
        {
            var slot = await this.appDbContext.Slots.FindAsync(id);
            if (slot != null)
            {
                this.appDbContext.Slots.Remove(slot);
                await this.appDbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public List<Slot> GetSlots(int recruiterId, DateTime date)
        {
            return this.appDbContext.Slots
                .Where(s => s.RecruiterId == recruiterId
                    && s.StartTime.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        /// <inheritdoc />
        public List<Slot> GetAllSlots(int recruiterId)
        {
            return this.appDbContext.Slots
                .Where(s => s.RecruiterId == recruiterId)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        /// <inheritdoc />
        public Slot? GetById(int id)
        {
            return this.appDbContext.Slots
                .FirstOrDefault(s => s.Id == id);
        }

        /// <inheritdoc />
        public void Add(Slot slot)
        {
            bool overlaps = this.appDbContext.Slots
                .Any(s => s.RecruiterId == slot.RecruiterId
                    && s.StartTime.Date == slot.StartTime.Date
                    && slot.StartTime < s.EndTime
                    && slot.EndTime > s.StartTime);

            if (overlaps)
            {
                throw new Exception("Slot overlaps with an existing appointment!");
            }

            this.appDbContext.Slots.Add(slot);
            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc />
        public void Update(Slot slot)
        {
            var existing = this.appDbContext.Slots.Find(slot.Id);
            if (existing == null)
            {
                throw new Exception("Slot not found");
            }

            existing.StartTime = slot.StartTime;
            existing.EndTime = slot.EndTime;
            existing.RecruiterId = slot.RecruiterId;
            existing.Duration = slot.Duration;
            existing.StatusValue = slot.StatusValue;
            existing.InterviewType = slot.InterviewType;

            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc />
        public void Delete(int id)
        {
            var slot = this.appDbContext.Slots.Find(id);
            if (slot != null)
            {
                this.appDbContext.Slots.Remove(slot);
                this.appDbContext.SaveChanges();
            }
        }
    }
}