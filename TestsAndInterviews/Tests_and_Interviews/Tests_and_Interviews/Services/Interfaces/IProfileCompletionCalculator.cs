using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IProfileCompletionCalculator
    {
        (int percentage, List<string> remainingTasks) Calculate(Company company);

        //(List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId);

        Task<(List<string> skillNames, List<int> percents)> GetSkillsTop3Async(int companyId);

        Task<string> ApplicantsMessage(int companyId);
    }
}