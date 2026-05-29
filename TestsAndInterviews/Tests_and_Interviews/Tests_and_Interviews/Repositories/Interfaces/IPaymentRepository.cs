namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews.Models;

    public interface IPaymentRepository
    {
        void UpdateJobPayment(int jobId, int paymentAmount);
        List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel);
        List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount);
    }
}