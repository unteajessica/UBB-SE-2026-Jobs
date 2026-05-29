using System;
using System.Collections.Generic;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    public class FakePaymentRepository : IPaymentRepository
    {
        private const string ExceptionMessage = "boom";

        public int LastUpdatedJobId { get; private set; }

        public int LastUpdatedAmount { get; private set; }

        public bool UpdateCalled { get; private set; }

        public bool ThrowOnUpdate { get; set; }

        public List<JobPaymentInfo> PaidJobsToReturn { get; } = new List<JobPaymentInfo>();

        public List<string> EmailsToNotify { get; } = new List<string>();

        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            if (ThrowOnUpdate)
            {
                throw new Exception(ExceptionMessage);
            }

            UpdateCalled = true;
            LastUpdatedJobId = jobId;
            LastUpdatedAmount = paymentAmount;
        }

        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            return PaidJobsToReturn;
        }

        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            return EmailsToNotify;
        }
    }
}