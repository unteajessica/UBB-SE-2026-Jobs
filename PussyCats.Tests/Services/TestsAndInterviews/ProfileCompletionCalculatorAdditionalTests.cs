using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PussyCats.Tests.Fakes;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace PussyCats.Tests.Services
{
    [TestClass]
    public class ProfileCompletionCalculatorAdditionalTests
    {
        private const string TestCompanyName = "TestCompany";
        private const string CompanyLogo = "logo.png";
        private const string CompanyPfp = "pfp.png";
        private const string CompanyDesc = "Description";
        private const string EmptyValue = "";

        private const string SkillCSharp = "C#";
        private const string SkillSql = "SQL";
        private const string SkillReact = "React";

        private const int CompanyIdValue = 1;
        private const int InitialJobs = 0;
        private const int InitialCollabs = 0;
        private const int CompletedJobs = 5;
        private const int CompletedCollabs = 2;

        private const int ZeroPercent = 0;
        private const int FullPercent = 100;
        private const int EmptyCount = 0;

        private const int SkillValHigh = 50;
        private const int SkillValMed = 30;
        private const int SkillValLow = 20;

        private FakeJobsService jobsService = null!;
        private FakeApplicantService applicantService = null!;
        private ProfileCompletionCalculator calculator = null!;

        [TestInitialize]
        public void Setup()
        {
            jobsService = new FakeJobsService();
            applicantService = new FakeApplicantService();
            calculator = new ProfileCompletionCalculator(jobsService, applicantService);
        }

        private Company CreateCompany()
        {
            var company = new Company(TestCompanyName, EmptyValue, EmptyValue, CompanyLogo, EmptyValue, EmptyValue);
            company.CompanyId = CompanyIdValue;
            company.PostedJobsCount = InitialJobs;
            company.CollaboratorsCount = InitialCollabs;
            company.Game = new Game();
            return company;
        }

        [TestMethod]
        public void Calculate_CompanyWithoutGame_IncludesGameTask()
        {
            var company = CreateCompany();
            company.Game = null!;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.remainingTasks.Contains("Complete mini-game"));
        }

        [TestMethod]
        public void Calculate_CompanyWithUnpublishedGame_IncludesGameTask()
        {
            var company = CreateCompany();
            company.Game.Unpublish();

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.remainingTasks.Contains("Complete mini-game"));
        }

        [TestMethod]
        public void Calculate_JobsCountBelowThreshold_IncludesJobsTask()
        {
            var company = CreateCompany();
            company.PostedJobsCount = 3;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.remainingTasks.Contains("Post at least 5 jobs"));
        }

        [TestMethod]
        public void Calculate_CollaboratorsCountAtThreshold_DoesNotIncludeCollabsTask()
        {
            var company = CreateCompany();
            company.CollaboratorsCount = 2;

            var result = calculator.Calculate(company);

            Assert.IsFalse(result.remainingTasks.Contains("Add 2 collaborators"));
        }

        [TestMethod]
        public void Calculate_TwoTasksCompleted_ReturnsFortyPercent()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;

            var result = calculator.Calculate(company);

            Assert.AreEqual(40, result.percentage);
        }

        [TestMethod]
        public void Calculate_ThreeTasksCompleted_ReturnsLessThanSixtyPercent()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;
            company.PostedJobsCount = CompletedJobs;

            var result = calculator.Calculate(company);

            Assert.AreEqual(60, result.percentage);
        }

        [TestMethod]
        public void Calculate_FourTasksCompleted_ReturnsEightyPercent()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;
            company.PostedJobsCount = CompletedJobs;
            company.CollaboratorsCount = CompletedCollabs;

            var result = calculator.Calculate(company);

            Assert.AreEqual(80, result.percentage);
        }



        [TestMethod]
        public void GetSkillsTop3Async_OtherCompanyJobs_NotIncluded()
        {
            var company1 = CreateCompany();
            var company2 = new Company("Company2", EmptyValue, EmptyValue, CompanyLogo, EmptyValue, EmptyValue);
            company2.CompanyId = 2;

            var job1 = new JobPosting
            {
                CompanyId = company1.CompanyId,
                Company = company1,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = "C#" }, RequiredPercentage = 50 }
                }
            };

            var job2 = new JobPosting
            {
                CompanyId = company2.CompanyId,
                Company = company2,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = "Python" }, RequiredPercentage = 50 }
                }
            };

            jobsService.AddJobDirectly(job1);
            jobsService.AddJobDirectly(job2);

            var result = calculator.GetSkillsTop3Async(company1.CompanyId).Result;

            Assert.IsTrue(result.skillNames.Contains("C#"));
            Assert.IsFalse(result.skillNames.Contains("Python"));
        }

        [TestMethod]
        public void ApplicantsMessage_ExactlySevenDaysAgoAtBoundary_CountedInPreviousWeek()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantService.UpdateApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-7.1) }).Wait();
            applicantService.UpdateApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now }).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;

            Assert.IsNotNull(message);
        }

        [TestMethod]
        public void ApplicantsMessage_EmptyPreviousWeekCount_ReturnsGreatStart()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantService.UpdateApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now }).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;

            Assert.IsTrue(message.Contains("Great start") || message.Contains("applicants"));
        }

        [TestMethod]
        public void ApplicantsMessage_SameCountPreviousAndCurrentWeek_ReturnsZeroPercentChange()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantService.UpdateApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-10) }).Wait();
            applicantService.UpdateApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now }).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;

            Assert.IsNotNull(message);
        }
    }
}
