namespace Tests_and_Interviews_API.Dtos
{
    using System.Collections.Generic;

    public class GameDto
    {
        public int AvatarId { get; set; }
        public string BuddyName { get; set; } = string.Empty;
        public string BuddyDescription { get; set; } = string.Empty;
        public string FinalQuote { get; set; } = string.Empty;
        public string Scen1Text { get; set; } = string.Empty;
        public string Scen1Answer1 { get; set; } = string.Empty;
        public string Scen1Answer2 { get; set; } = string.Empty;
        public string Scen1Answer3 { get; set; } = string.Empty;
        public string Scen1Reaction1 { get; set; } = string.Empty;
        public string Scen1Reaction2 { get; set; } = string.Empty;
        public string Scen1Reaction3 { get; set; } = string.Empty;
        public string Scen2Text { get; set; } = string.Empty;
        public string Scen2Answer1 { get; set; } = string.Empty;
        public string Scen2Answer2 { get; set; } = string.Empty;
        public string Scen2Answer3 { get; set; } = string.Empty;
        public string Scen2Reaction1 { get; set; } = string.Empty;
        public string Scen2Reaction2 { get; set; } = string.Empty;
        public string Scen2Reaction3 { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
    }
}