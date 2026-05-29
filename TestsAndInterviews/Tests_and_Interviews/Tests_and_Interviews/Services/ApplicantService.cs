// <copyright file="ApplicantService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services.Interfaces;

    public class ApplicantService : IApplicantService
    {
        private readonly HttpClient http;

        private const decimal PassIndividual = 5.5m;
        private const decimal PassCollective = 7.0m;
        private const int MinimumNameLength = 2;
        private const int MinimumSkillsLength = 3;
        private const int MinimumInterestsLength = 3;
        private const int MinimumPhoneDigits = 8;
        private const int MinimumSummaryLength = 20;
        private const int MinimumProjectsLength = 15;
        private const int MinimumSkillsWordCount = 1;
        private const int MinimumProjectsWordCount = 2;
        private const int TotalExpectedGradesCount = 4;
        private const decimal BaseTotalGrade = 3.5m;
        private const decimal MaximumTotalGrade = 10.0m;
        private const decimal InitialWordPointValue = 0.5m;
        private const decimal WordPointDecayMultiplier = 0.7m;
        private const decimal SkillsSectionWeight = 1.35m;
        private const decimal InterestsSectionWeight = 0.55m;
        private const decimal SummarySectionWeight = 1.15m;
        private const decimal ProjectsSectionWeight = 1.35m;
        private const string ApplicationStatusRejected = "Rejected";
        private const string ApplicationStatusOnHold = "On Hold";
        private const string XmlElementName = "Name";
        private const string XmlElementEmail = "Email";
        private const string XmlElementSkills = "Skills";
        private const string XmlElementInterests = "Interests";
        private const string XmlElementPhone = "Phone";
        private const string XmlElementContactNumber = "ContactNumber";
        private const string XmlElementSummary = "Summary";
        private const string XmlElementProjects = "Projects";
        private const char AtSymbolCharacter = '@';
        private const char DotCharacter = '.';
        private const char SpaceCharacter = ' ';
        private const string SpaceString = " ";
        private static readonly char[] TextSplitCharacters = new[] { ' ', ',', ';' };
        private static readonly List<string> StopWordsList = new List<string> { "the", "and", "is", "a", "an", "in", "to", "of", "for" };
        private static readonly List<string> DefaultKeywordsList = new List<string> { "c#", "java", "sql", "react", "agile", "javascript", ".net", "python", "docker", "azure" };
        private static readonly Dictionary<string, string> SynonymDictionaryMap = new Dictionary<string, string>
        {
            { "csharp", "c#" },
            { "js", "javascript" },
            { "reactjs", "react" },
            { "dot net", ".net" },
            { "dotnet", ".net" }
        };

        public ApplicantService()
        {
            this.http = ApiClient.Http;
        }

        public ApplicantService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsForJob(JobPosting job)
        {
            HttpResponseMessage response = await this.http.GetAsync($"applicants/byjob/{job.JobId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Applicant>();
            }

            response.EnsureSuccessStatusCode();
            List<ApplicantDto>? dtos = await response.Content.ReadFromJsonAsync<List<ApplicantDto>>();
            return dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<Applicant>();
        }

        public async Task<Applicant> GetApplicant(int applicantId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"applicants/{applicantId}");
            response.EnsureSuccessStatusCode();
            ApplicantDto? dto = await response.Content.ReadFromJsonAsync<ApplicantDto>();
            return dto!.ToEntity();
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsByCompany(int companyId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"applicants/bycompany/{companyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Applicant>();
            }

            response.EnsureSuccessStatusCode();
            List<ApplicantDto>? dtos = await response.Content.ReadFromJsonAsync<List<ApplicantDto>>();
            return dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<Applicant>();
        }

        public async Task UpdateCompanyTestGrade(int applicantId, decimal grade)
        {
            Applicant applicant = await this.GetApplicant(applicantId);
            if (applicant != null)
            {
                applicant.CompanyTestGrade = grade;
                EvaluateApplicantStatus(applicant);
                await this.UpdateApplicantViaApiAsync(applicant);
            }
        }

        public async Task UpdateInterviewGrade(int applicantId, decimal grade)
        {
            Applicant applicant = await this.GetApplicant(applicantId);
            if (applicant != null)
            {
                applicant.InterviewGrade = grade;
                EvaluateApplicantStatus(applicant);
                await this.UpdateApplicantViaApiAsync(applicant);
            }
        }

        public async Task ProcessCv(int applicantId)
        {
            Applicant applicant = await this.GetApplicant(applicantId);
            if (applicant == null)
            {
                return;
            }
            decimal? curriculumVitaeGrade = await this.ScanCvXmlAsync(applicant);
            if (curriculumVitaeGrade != null)
            {
                applicant.CvGrade = curriculumVitaeGrade;
            }
            EvaluateApplicantStatus(applicant);
            await this.UpdateApplicantViaApiAsync(applicant);
        }

        public async Task UpdateAppTestGrade(int applicantId, decimal grade)
        {
            Applicant applicant = await this.GetApplicant(applicantId);
            if (applicant != null)
            {
                applicant.AppTestGrade = grade;
                EvaluateApplicantStatus(applicant);
                await this.UpdateApplicantViaApiAsync(applicant);
            }
        }

        public async Task<decimal?> ScanCvXmlAsync(Applicant applicant)
        {
            // Fetch CvXml from user endpoint
            HttpResponseMessage userResponse = await this.http.GetAsync($"users/{applicant.UserId}");
            userResponse.EnsureSuccessStatusCode();
            UserDto? userDto = await userResponse.Content.ReadFromJsonAsync<UserDto>();
            string? curriculumVitaeXml = userDto?.CvXml;

            if (ValidateCurriculumVitae(curriculumVitaeXml) == false)
            {
                return null;
            }

            // Fetch job skills for this applicant's job
            HttpResponseMessage jobSkillsResponse = await this.http.GetAsync($"jobs/{applicant.JobId}/skills");
            jobSkillsResponse.EnsureSuccessStatusCode();
            List<JobSkillDto>? jobSkillDtos = await jobSkillsResponse.Content.ReadFromJsonAsync<List<JobSkillDto>>();

            // Fetch all skills to resolve SkillId -> SkillName
            HttpResponseMessage allSkillsResponse = await this.http.GetAsync("jobs/skills");
            allSkillsResponse.EnsureSuccessStatusCode();
            List<SkillDto>? allSkillDtos = await allSkillsResponse.Content.ReadFromJsonAsync<List<SkillDto>>();
            Dictionary<int, string> skillNameMap = allSkillDtos?.ToDictionary(s => s.SkillId, s => s.SkillName)
                ?? new Dictionary<int, string>();

            List<string> expectedKeywordsList = new List<string>();
            if (jobSkillDtos != null)
            {
                foreach (var jobSkill in jobSkillDtos)
                {
                    if (skillNameMap.TryGetValue(jobSkill.SkillId, out string? skillName) && !string.IsNullOrWhiteSpace(skillName))
                    {
                        expectedKeywordsList.Add(skillName.ToLower());
                    }
                }
            }

            if (expectedKeywordsList.Count == 0)
            {
                expectedKeywordsList = DefaultKeywordsList;
            }

            XDocument xmlDocument = XDocument.Parse(curriculumVitaeXml!);
            decimal totalGrade = BaseTotalGrade;
            totalGrade += ScoreCurriculumVitaeSection(xmlDocument, XmlElementSkills, expectedKeywordsList, SkillsSectionWeight);
            totalGrade += ScoreCurriculumVitaeSection(xmlDocument, XmlElementInterests, expectedKeywordsList, InterestsSectionWeight);
            totalGrade += ScoreCurriculumVitaeSection(xmlDocument, XmlElementSummary, expectedKeywordsList, SummarySectionWeight);
            totalGrade += ScoreCurriculumVitaeSection(xmlDocument, XmlElementProjects, expectedKeywordsList, ProjectsSectionWeight);

            if (totalGrade > MaximumTotalGrade)
            {
                return MaximumTotalGrade;
            }
            return totalGrade;
        }

        public async Task UpdateApplicant(Applicant applicant)
        {
            EvaluateApplicantStatus(applicant);
            await this.UpdateApplicantViaApiAsync(applicant);
        }

        public async Task RemoveApplicant(int applicantId)
        {
            HttpResponseMessage response = await this.http.DeleteAsync($"applicants/{applicantId}");
            response.EnsureSuccessStatusCode();
        }

        private async Task UpdateApplicantViaApiAsync(Applicant applicant)
        {
            HttpResponseMessage response = await this.http.PutAsJsonAsync(
                $"applicants/{applicant.ApplicantId}",
                applicant.ToDto());
            response.EnsureSuccessStatusCode();
        }

        private static bool TryGetTrimmedElement(XDocument xmlDocument, string localName, out string elementValue)
        {
            elementValue = string.Empty;
            var xmlElement = xmlDocument.Descendants(localName).FirstOrDefault();
            if (xmlElement == null || string.IsNullOrWhiteSpace(xmlElement.Value))
            {
                return false;
            }
            elementValue = xmlElement.Value.Trim();
            return true;
        }

        private static bool ValidateEmailFormat(string emailAddress)
        {
            var atSymbolIndex = emailAddress.IndexOf(AtSymbolCharacter, StringComparison.Ordinal);
            if (atSymbolIndex <= 0 || atSymbolIndex >= emailAddress.Length - 1)
            {
                return false;
            }
            var domainString = emailAddress[(atSymbolIndex + 1)..];
            return domainString.Contains(DotCharacter, StringComparison.Ordinal);
        }

        private static bool ValidateMeaningfulSkillsText(string skillsText)
        {
            if (skillsText.Length < MinimumSkillsLength)
            {
                return false;
            }
            var stringParts = skillsText.Split(TextSplitCharacters, StringSplitOptions.RemoveEmptyEntries);
            return stringParts.Length >= MinimumSkillsWordCount;
        }

        private static bool ValidateMeaningfulProjectsText(string projectsText)
        {
            if (projectsText.Length < MinimumProjectsLength)
            {
                return false;
            }
            var stringParts = projectsText.Split(TextSplitCharacters, StringSplitOptions.RemoveEmptyEntries);
            return stringParts.Length >= MinimumProjectsWordCount;
        }

        private static bool ValidatePlausiblePhone(string phoneText)
        {
            var digitCount = phoneText.Count(char.IsDigit);
            return digitCount >= MinimumPhoneDigits;
        }

        private bool ValidateCurriculumVitae(string? curriculumVitaeXml)
        {
            if (string.IsNullOrWhiteSpace(curriculumVitaeXml))
            {
                return false;
            }
            try
            {
                XDocument xmlDocument = XDocument.Parse(curriculumVitaeXml);
                if (!TryGetTrimmedElement(xmlDocument, XmlElementName, out var nameString) || nameString.Length < MinimumNameLength)
                {
                    return false;
                }
                if (!TryGetTrimmedElement(xmlDocument, XmlElementEmail, out var emailString) || !ValidateEmailFormat(emailString))
                {
                    return false;
                }
                if (!TryGetTrimmedElement(xmlDocument, XmlElementSkills, out var skillsString) || !ValidateMeaningfulSkillsText(skillsString))
                {
                    return false;
                }
                if (!TryGetTrimmedElement(xmlDocument, XmlElementInterests, out var interestsString) || interestsString.Length < MinimumInterestsLength)
                {
                    return false;
                }
                string phoneString;
                if (!TryGetTrimmedElement(xmlDocument, XmlElementPhone, out phoneString) && !TryGetTrimmedElement(xmlDocument, XmlElementContactNumber, out phoneString))
                {
                    return false;
                }
                if (!ValidatePlausiblePhone(phoneString))
                {
                    return false;
                }
                if (!TryGetTrimmedElement(xmlDocument, XmlElementSummary, out var summaryString) || summaryString.Length < MinimumSummaryLength)
                {
                    return false;
                }
                if (!TryGetTrimmedElement(xmlDocument, XmlElementProjects, out var projectsString) || !ValidateMeaningfulProjectsText(projectsString))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<string> TokenizeAndClean(string textToClean)
        {
            string cleanedText = string.Empty;
            foreach (char character in textToClean)
            {
                if (char.IsPunctuation(character))
                {
                    cleanedText += SpaceString;
                }
                else
                {
                    cleanedText += character;
                }
            }
            string[] rawWordsArray = cleanedText.Split(new char[] { SpaceCharacter }, StringSplitOptions.RemoveEmptyEntries);
            List<string> finalWordsList = new List<string>();
            foreach (string word in rawWordsArray)
            {
                string lowercasedWord = word.ToLower();
                if (StopWordsList.Contains(lowercasedWord) == false)
                {
                    finalWordsList.Add(lowercasedWord);
                }
            }
            return finalWordsList;
        }

        private List<string> ApplySynonyms(List<string> wordsList)
        {
            List<string> newWordsList = new List<string>();
            foreach (string word in wordsList)
            {
                if (SynonymDictionaryMap.ContainsKey(word))
                {
                    newWordsList.Add(SynonymDictionaryMap[word]);
                }
                else
                {
                    newWordsList.Add(word);
                }
            }
            return newWordsList;
        }

        private decimal CalculateTermFrequencyInverseDocumentFrequencyGrade(List<string> wordsList, List<string> importantKeywordsList, decimal sectionWeight)
        {
            Dictionary<string, int> termFrequenciesDictionary = new Dictionary<string, int>();
            foreach (string word in wordsList)
            {
                if (importantKeywordsList.Contains(word))
                {
                    if (termFrequenciesDictionary.ContainsKey(word))
                    {
                        termFrequenciesDictionary[word] += 1;
                    }
                    else
                    {
                        termFrequenciesDictionary[word] = 1;
                    }
                }
            }
            decimal totalScore = 0;
            foreach (var keywordAppearance in termFrequenciesDictionary)
            {
                int keywordCount = keywordAppearance.Value;
                decimal wordScore = 0;
                decimal currentPointValue = InitialWordPointValue;
                for (int index = 0; index < keywordCount; index++)
                {
                    wordScore += currentPointValue;
                    currentPointValue = currentPointValue * WordPointDecayMultiplier;
                }
                totalScore += wordScore;
            }
            return totalScore * sectionWeight;
        }

        private decimal ScoreCurriculumVitaeSection(XDocument xmlDocument, string elementName, List<string> expectedKeywordsList, decimal sectionWeight)
        {
            var xmlNode = xmlDocument.Descendants(elementName).FirstOrDefault();
            if (xmlNode == null || string.IsNullOrWhiteSpace(xmlNode.Value))
            {
                return 0m;
            }
            List<string> wordsList = TokenizeAndClean(xmlNode.Value);
            wordsList = ApplySynonyms(wordsList);
            return CalculateTermFrequencyInverseDocumentFrequencyGrade(wordsList, expectedKeywordsList, sectionWeight);
        }

        private void EvaluateApplicantStatus(Applicant applicant)
        {
            List<decimal> nonNullGradesList = new List<decimal>();
            if (applicant.AppTestGrade != null)
            {
                nonNullGradesList.Add(applicant.AppTestGrade.Value);
            }
            if (applicant.CvGrade != null)
            {
                nonNullGradesList.Add(applicant.CvGrade.Value);
            }
            if (applicant.CompanyTestGrade != null)
            {
                nonNullGradesList.Add(applicant.CompanyTestGrade.Value);
            }
            if (applicant.InterviewGrade != null)
            {
                nonNullGradesList.Add(applicant.InterviewGrade.Value);
            }
            foreach (decimal grade in nonNullGradesList)
            {
                if (grade < PassIndividual)
                {
                    applicant.ApplicationStatus = ApplicationStatusRejected;
                    return;
                }
            }
            if (nonNullGradesList.Count > 0)
            {
                decimal sumOfGrades = 0;
                foreach (decimal grade in nonNullGradesList)
                {
                    sumOfGrades += grade;
                }
                decimal averageGrade = sumOfGrades / nonNullGradesList.Count;
                if (averageGrade < PassCollective)
                {
                    applicant.ApplicationStatus = ApplicationStatusRejected;
                    return;
                }
            }
            if (nonNullGradesList.Count == TotalExpectedGradesCount && string.IsNullOrEmpty(applicant.ApplicationStatus))
            {
                applicant.ApplicationStatus = ApplicationStatusOnHold;
            }
        }
    }
}