using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> ProcessPaymentAsync(int jobId, int amount, string name, string cardNum, string exp, string cvv);
        Task<List<JobPaymentInfo>> GetPaidJobsInfo(string jobType, string expLevel);
    }
}