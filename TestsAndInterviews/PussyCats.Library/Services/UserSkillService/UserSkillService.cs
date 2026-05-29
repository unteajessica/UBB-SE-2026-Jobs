using PussyCats.Library.Repositories.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.UserSkillService
{
    public class UserSkillService : IUserSkillService
    {
        private readonly IUserSkillRepository repository;

        public UserSkillService(IUserSkillRepository repository)
        {
            this.repository = repository;
        }


        public Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken=default) 
            => repository.GetAsync(userId, skillId, cancellationToken);

        public Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken=default)
            => repository.GetByUserIdAsync(userId, cancellationToken);

        public Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken ct = default)
        => repository.GetVerifiedByUserIdAsync(userId, ct);

        public Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default)
            => repository.AddAsync(userSkill, ct);

        public Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default)
            => repository.UpdateAsync(userSkill, ct);

        public Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken ct = default)
            => repository.UpdateScoreAsync(userId, skillId, score, ct);

        public Task RemoveAsync(int userId, int skillId, CancellationToken ct = default)
            => repository.RemoveAsync(userId, skillId, ct);
    }
}
