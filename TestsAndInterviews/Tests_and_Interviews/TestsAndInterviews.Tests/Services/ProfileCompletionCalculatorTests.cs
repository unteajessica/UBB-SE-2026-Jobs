using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using TestsAndInterviews.Tests.Helpers;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TestsAndInterviews.Tests.Services
{
    [TestClass]
    public class ProfileCompletionCalculatorTests
    {
        private const string TestCompanyName = "TestCompany";
        private const string CompanyLogo = "logo.png";
        private const string CompanyPfp = "pfp.png";
        private const string CompanyDesc = "Description";
        private const string EmptyValue = "";

        private const string SkillCSharp = "C#";
        private const string SkillSql = "SQL";
        private const string SkillReact = "React";

        private const string MsgNoApplicants = "No applicants yet. Start posting jobs!";
        private const string MsgStartKeyword = "Great start";
        private const string MsgCongratsKeyword = "Congrats";
        private const string MsgDecreaseKeyword = "fewer";

        private const int CompanyIdValue = 1;
        private const int InitialJobs = 0;
        private const int InitialCollabs = 0;
        private const int CompletedJobs = 5;
        private const int CompletedCollabs = 2;

        private const int ZeroPercent = 0;
        private const int FullPercent = 100;
        private const int EmptyCount = 0;
        private const int ExpectedTopSkillsCount = 3;

        private const int SkillValHigh = 50;
        private const int SkillValMed = 30;
        private const int SkillValLow = 20;

        private const int OffsetPastDaysFar = -10;
        private const int OffsetPastDaysNear = -8;

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
        public void Calculate_EmptyCompany_ReturnsZero()
        {
            var company = CreateCompany();
            var result = calculator.Calculate(company);
            Assert.AreEqual(ZeroPercent, result.percentage);
            Assert.IsTrue(result.remainingTasks.Count > EmptyCount);
        }

        [TestMethod]
        public void Calculate_AllTasksCompleted_Returns100()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;
            company.PostedJobsCount = CompletedJobs;
            company.CollaboratorsCount = CompletedCollabs;
            company.Game.Publish();

            var result = calculator.Calculate(company);

            Assert.AreEqual(FullPercent, result.percentage);
            Assert.AreEqual(EmptyCount, result.remainingTasks.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_NoJobs_ReturnsEmptyLists()
        {
            var result = calculator.GetSkillsTop3Async(CompanyIdValue).Result;
            Assert.AreEqual(EmptyCount, result.skillNames.Count);
            Assert.AreEqual(EmptyCount, result.percents.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_ReturnsTopSkills()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                CompanyId = company.CompanyId,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = SkillCSharp }, RequiredPercentage = SkillValHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = SkillValMed },
                    new JobSkill { Skill = new Skill { SkillName = SkillReact }, RequiredPercentage = SkillValLow }
                }
            };

            jobsService.AddJobDirectly(job);

            var result = calculator.GetSkillsTop3Async(company.CompanyId).Result;
            Assert.AreEqual(ExpectedTopSkillsCount, result.skillNames.Count);
        }

        [TestMethod]
        public void ApplicantsMessage_NoApplicants_ReturnsStartMessage()
        {
            var message = calculator.ApplicantsMessage(CompanyIdValue).Result;
            Assert.AreEqual(MsgNoApplicants, message);
        }

        [TestMethod]
        public void ApplicantsMessage_FirstApplicants_ReturnsGreatStart()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company, CompanyId = company.CompanyId };

            var applicant = new Applicant
            {
                Job = job,
                AppliedAt = DateTime.Now
            };
            applicantService.UpdateApplicant(applicant).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;
            Assert.IsTrue(message.Contains(MsgStartKeyword));
        }

        [TestMethod]
        public void ApplicantsMessage_MoreApplicants_ReturnsCongratsMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company, CompanyId = company.CompanyId };

            applicantService.UpdateApplicant(new Applicant { ApplicantId = 1, Job = job, AppliedAt = DateTime.Now }).Wait();
            applicantService.UpdateApplicant(new Applicant { ApplicantId = 2, Job = job, AppliedAt = DateTime.Now }).Wait();
            applicantService.UpdateApplicant(new Applicant { ApplicantId = 3, Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) }).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;
            Assert.IsTrue(message.Contains(MsgCongratsKeyword));
        }

        [TestMethod]
        public void ApplicantsMessage_FewerApplicants_ReturnsDecreaseMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company, CompanyId = company.CompanyId };

            applicantService.UpdateApplicant(new Applicant { ApplicantId = 1, Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) }).Wait();
            applicantService.UpdateApplicant(new Applicant { ApplicantId = 2, Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) }).Wait();
            applicantService.UpdateApplicant(new Applicant { ApplicantId = 3, Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) }).Wait();
            applicantService.UpdateApplicant(new Applicant { ApplicantId = 4, Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysNear) }).Wait();

            var message = calculator.ApplicantsMessage(company.CompanyId).Result;
            Assert.IsTrue(message.Contains(MsgDecreaseKeyword));
        }

        [TestMethod]
        public void Calculate_WithPicture_IncrementsPercentage()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent);
            Assert.IsFalse(result.remainingTasks.Contains("Upload company picture"));
        }

        [TestMethod]
        public void Calculate_WithAboutUs_IncrementsPercentage()
        {
            var company = CreateCompany();
            company.AboutUs = CompanyDesc;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent);
            Assert.IsFalse(result.remainingTasks.Contains("Add company description"));
        }

        [TestMethod]
        public void Calculate_WithJobs_IncrementsPercentage()
        {
            var company = CreateCompany();
            company.PostedJobsCount = CompletedJobs;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent);
            Assert.IsFalse(result.remainingTasks.Contains("Post at least 5 jobs"));
        }

        [TestMethod]
        public void Calculate_WithCollaborators_IncrementsPercentage()
        {
            var company = CreateCompany();
            company.CollaboratorsCount = CompletedCollabs;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent);
            Assert.IsFalse(result.remainingTasks.Contains("Add 2 collaborators"));
        }

        [TestMethod]
        public void Calculate_WithPublishedGame_IncrementsPercentage()
        {
            var company = CreateCompany();
            company.Game.Publish();

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent);
            Assert.IsFalse(result.remainingTasks.Contains("Complete mini-game"));
        }

        [TestMethod]
        public void Calculate_WithPartialCompletion_ReturnsCorrectPercentage()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;
            company.PostedJobsCount = CompletedJobs;

            var result = calculator.Calculate(company);

            Assert.IsTrue(result.percentage > ZeroPercent && result.percentage < FullPercent);
            Assert.IsTrue(result.remainingTasks.Count > EmptyCount && result.remainingTasks.Count < 5);
        }

        [TestMethod]
        public void GetSkillsTop3_JobWithNullSkills_Ignores()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                JobSkills = null
            };

            jobsService.AddJobDirectly(job);

            var result = calculator.GetSkillsTop3Async(company.CompanyId).Result;

            Assert.AreEqual(0, result.skillNames.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_JobWithEmptySkills_Ignores()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                JobSkills = new List<JobSkill>()
            };

            jobsService.AddJobDirectly(job);

            var result = calculator.GetSkillsTop3Async(company.CompanyId).Result;

            Assert.AreEqual(0, result.skillNames.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_SkillWithNullName_Ignores()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = null }, RequiredPercentage = SkillValHigh }
                }
            };

            jobsService.AddJobDirectly(job);

            var result = calculator.GetSkillsTop3Async(company.CompanyId).Result;

            Assert.AreEqual(0, result.skillNames.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_Sync_NoJobs_ReturnsEmptyLists()
        {
            var result = calculator.GetSkillsTop3(CompanyIdValue);
            Assert.AreEqual(EmptyCount, result.skillNames.Count);
            Assert.AreEqual(EmptyCount, result.percents.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_Sync_ReturnsTopSkills()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                CompanyId = company.CompanyId,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = SkillCSharp }, RequiredPercentage = SkillValHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = SkillValMed },
                    new JobSkill { Skill = new Skill { SkillName = SkillReact }, RequiredPercentage = SkillValLow }
                }
            };

            jobsService.AddJobDirectly(job);

            var result = calculator.GetSkillsTop3(company.CompanyId);
            Assert.AreEqual(ExpectedTopSkillsCount, result.skillNames.Count);
            Assert.AreEqual(SkillCSharp, result.skillNames[0]);
        }
    }
}