using PussyCats.Library.Services;

namespace PussyCats.App.Services;

public static class UserLevelService
{
    private const int FullProgressPercentage = 100;
    private const int NoExperiencePointsRemaining = 0;

    public static int GetLevelProgressPercent(int totalExperiencePoints, int currentLevel)
    {
        if (totalExperiencePoints < 0)
        {
            throw new ArgumentException("Experience Points cannot be negative.");
        }

        int nextLevelExperiencePoints = GetNextLevelExperiencePoints(currentLevel);
        if (nextLevelExperiencePoints == SimpleModelOperations.Level1ExperiencePoints)
        {
            return FullProgressPercentage;
        }

        double completedPercentageIntoCurrentLevel = GetLevelProgressPercentage(totalExperiencePoints, currentLevel);
        return (int)completedPercentageIntoCurrentLevel;
    }

    private static double GetLevelProgressPercentage(int totalExperiencePoints, int currentLevel)
    {
        int experiencePointsRequired = GetExperiencePointsRequiredForLevel(currentLevel);
        int nextLevelExperiencePoints = GetNextLevelExperiencePoints(currentLevel);
        double pointsIntoLevel = totalExperiencePoints - experiencePointsRequired;
        double totalPointsForLevel = nextLevelExperiencePoints - experiencePointsRequired;
        double completedPercentageIntoCurrentLevel = pointsIntoLevel / totalPointsForLevel * FullProgressPercentage;
        return completedPercentageIntoCurrentLevel;
    }

    public static int GetExperiencePointsToNextLevel(int totalExperiencePoints, int currentLevel)
    {
        if (totalExperiencePoints < 0)
        {
            throw new ArgumentException("Experience Points cannot be negative.");
        }

        int nextLevelExperiencePoints = GetNextLevelExperiencePoints(currentLevel);
        if (nextLevelExperiencePoints == SimpleModelOperations.Level1ExperiencePoints)
        {
            return NoExperiencePointsRemaining;
        }
        return nextLevelExperiencePoints - totalExperiencePoints;
    }

    public static int CalculateLevelNumber(int experiencePoints)
    {
        if (experiencePoints < 0)
        {
            throw new ArgumentException("Experience Points cannot be negative.");
        }
        return SimpleModelOperations.CalculateLevelNumber(experiencePoints);
    }

    public static int GetExperiencePointsRequiredForLevel(int level)
    {
        return level switch
        {
            >= 5 => SimpleModelOperations.Level5ExperiencePoints,
            4 => SimpleModelOperations.Level4ExperiencePoints,
            3 => SimpleModelOperations.Level3ExperiencePoints,
            2 => SimpleModelOperations.Level2ExperiencePoints,
            _ => SimpleModelOperations.Level1ExperiencePoints,
        };
    }

    public static int GetNextLevelExperiencePoints(int currentLevel)
    {
        return currentLevel switch
        {
            >= 5 => SimpleModelOperations.Level1ExperiencePoints,
            4 => SimpleModelOperations.Level5ExperiencePoints,
            3 => SimpleModelOperations.Level4ExperiencePoints,
            2 => SimpleModelOperations.Level3ExperiencePoints,
            _ => SimpleModelOperations.Level2ExperiencePoints,
        };
    }
}
