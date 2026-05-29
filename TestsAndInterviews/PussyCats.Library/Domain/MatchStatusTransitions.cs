using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public static class MatchStatusTransitions
{
    public static bool IsDecisionTransitionAllowed(MatchStatus current, MatchStatus next)
    {
        if (current == MatchStatus.Applied)
            return next is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;

        if (current == MatchStatus.Advanced)
            return next is MatchStatus.Accepted or MatchStatus.Rejected;

        return false;
    }
}
