using PussyCats.Web.Dtos;

namespace PussyCats.Web.Models
{
    public class LeaderboardViewModel
    {
        public int TestId { get; set; }

        public List<LeaderboardEntryDto> Entries { get; set; } = new();

        public LeaderboardEntryDto? CurrentUserEntry { get; set; }

        public List<LeaderboardEntryDto> TopEntries =>
            this.Entries
                .OrderBy(entry => entry.RankPosition)
                .Take(3)
                .ToList();
    }
}
