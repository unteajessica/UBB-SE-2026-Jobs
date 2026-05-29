using System.Collections.Generic;
using Tests_and_Interviews_API.Models;

namespace Tests_and_Interviews_API.Services.Interfaces
{
    public interface IProfileCompletionCalculator
    {
        (int percentage, List<string> remainingTasks) Calculate(Company company);

        (List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId);

        string ApplicantsMessage(int companyId);
    }
}