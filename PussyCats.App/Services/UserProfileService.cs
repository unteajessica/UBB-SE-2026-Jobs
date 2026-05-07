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

    public async Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await skillTestRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsProfileAvailableAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            throw new Exception($"No profile found for ID {userId}");
        }

        return user.ActiveAccount;
    }

    public async Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        await userRepository.UpdateActiveAccountAsync(userId, isActive, cancellationToken).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateProfilePicturePathAsync(int userId, string newPath, CancellationToken cancellationToken = default)
    {
        await userRepository.UpdateProfilePicturePathAsync(userId, newPath ?? string.Empty, cancellationToken).ConfigureAwait(false);
        await userRepository.TouchLastUpdatedAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveProfilePicturePathAsync(int userId, CancellationToken cancellationToken = default)
    {
        await UpdateProfilePicturePathAsync(userId, string.Empty, cancellationToken).ConfigureAwait(false);
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
        parsedCvTextBuilder.AppendLine(string.Join(", ", user.Skills.Select(skill => skill.Skill?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name))));
        return parsedCvTextBuilder.ToString().TrimEnd();
    }

    public async Task SaveAsync(int userId, User user, CancellationToken cancellationToken = default)
    {
        var existing = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            await userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<int> RecalculateLevelAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user is null)
        {
            return 0;
        }

        var skillTests = await skillTestRepository.GetByUserIdAsync(user.UserId, cancellationToken).ConfigureAwait(false);
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
