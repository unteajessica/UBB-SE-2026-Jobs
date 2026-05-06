using System.Text;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository userRepository;
    private readonly ISkillTestRepository skillTestRepository;

    public UserProfileService(IUserRepository userRepository, ISkillTestRepository skillTestRepository)
    {
        this.userRepository = userRepository;
        this.skillTestRepository = skillTestRepository;
    }

    public async Task<User?> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        return await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken ct = default)
    {
        return await skillTestRepository.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<bool> IsProfileAvailableAsync(int userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);

        if (user is null)
        {
            throw new Exception($"No profile found for ID {userId}");
        }

        return user.ActiveAccount;
    }

    public async Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        await userRepository.UpdateActiveAccountAsync(userId, isActive, ct).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task UpdateProfilePicturePathAsync(int userId, string newPath, CancellationToken ct = default)
    {
        await userRepository.UpdateProfilePicturePathAsync(userId, newPath ?? string.Empty, ct).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task RemoveProfilePicturePathAsync(int userId, CancellationToken ct = default)
    {
        await UpdateProfilePicturePathAsync(userId, string.Empty, ct).ConfigureAwait(false);
    }

    public string GenerateParsedCvText(User user)
    {
        if (user is null)
        {
            return string.Empty;
        }

        var parsedCvTextBuilder = new StringBuilder();
        parsedCvTextBuilder.AppendLine($"{user.FirstName} {user.LastName}".Trim());
        parsedCvTextBuilder.AppendLine(user.University ?? string.Empty);
        parsedCvTextBuilder.AppendLine(string.Join(", ", user.Skills.Select(s => s.Skill?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n))));
        return parsedCvTextBuilder.ToString().TrimEnd();
    }

    public async Task SaveAsync(int userId, User user, CancellationToken ct = default)
    {
        var existing = await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
        if (existing is null)
        {
            await userRepository.AddAsync(user, ct).ConfigureAwait(false);
        }
        else
        {
            await userRepository.UpdateAsync(user, ct).ConfigureAwait(false);
        }
    }

    public async Task<int> RecalculateLevelAsync(User user, CancellationToken ct = default)
    {
        if (user is null)
        {
            return 0;
        }

        var skillTests = await skillTestRepository.GetByUserIdAsync(user.UserId, ct).ConfigureAwait(false);
        int totalExperiencePoints = 0;

        foreach (var skillTest in skillTests)
        {
            totalExperiencePoints += SimpleModelOperations.GetExperiencePoints(skillTest);
        }

        user.TotalExperiencePoints = totalExperiencePoints;
        user.CurrentLevel = SimpleModelOperations.CalculateLevelNumber(totalExperiencePoints);

        return totalExperiencePoints;
    }
}
