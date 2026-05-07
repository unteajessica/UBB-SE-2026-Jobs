using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

/// <summary>Candidate profile read/write, including CV text generation and level recalculation.</summary>
public interface IUserProfileService
{
    Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> IsProfileAvailableAsync(int userId, CancellationToken cancellationToken = default);

    Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default);

    Task UpdateProfilePicturePathAsync(int userId, string newPath, CancellationToken cancellationToken = default);

    Task RemoveProfilePicturePathAsync(int userId, CancellationToken cancellationToken = default);

    string GenerateParsedCvText(User user);

    Task SaveAsync(int userId, User user, CancellationToken cancellationToken = default);

    Task<int> RecalculateLevelAsync(User user, CancellationToken cancellationToken = default);
}
