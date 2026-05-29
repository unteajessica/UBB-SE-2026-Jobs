namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;

    public partial class SkillPickItem : ObservableObject
    {
        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private string requiredPercentText = "50";

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillPickItem"/> class.
        /// </summary>
        /// <param name="skill">The skill model.</param>
        public SkillPickItem(Skill skill)
        {
            this.Skill = skill;
        }

        /// <summary>
        /// Gets the underlying skill of this item.
        /// </summary>
        public Skill Skill { get; }
    }

    public partial class CreateJobViewModel : ObservableObject
    {
        private readonly IJobsService jobsService;
        private readonly SessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateJobViewModel"/> class.
        /// </summary>
        /// <param name="jobsService">The jobs repository.</param>
        /// <param name="sessionService">The session service.</param>
        public CreateJobViewModel(IJobsService jobsService, SessionService sessionService)
        {
            this.jobsService = jobsService;
            this.sessionService = sessionService;

            //foreach (var skillItem in await this.jobsService.GetAllSkillsAsync())
            //{
            //    this.SkillRows.Add(new SkillPickItem(skillItem));
            //}
        }

        public async Task InitializeAsync()
        {
            var skills = await this.jobsService.GetAllSkillsAsync();

            foreach (var skillitem in skills)
            {
                this.SkillRows.Add(new SkillPickItem(skillitem));
            }
        }

        /// <summary>
        /// Gets the collection of skill picker items.
        /// </summary>
        public ObservableCollection<SkillPickItem> SkillRows { get; } = new();

        /// <summary>
        /// Gets or sets the action to invoke when the save operation completes.
        /// </summary>
        public Action<bool, string>? OnSaveCompleted { get; set; }

        [ObservableProperty]
        private string jobTitle = string.Empty;

        [ObservableProperty]
        private string industryField = string.Empty;

        [ObservableProperty]
        private string jobType = string.Empty;

        [ObservableProperty]
        private string experienceLevel = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? startDate;

        [ObservableProperty]
        private DateTimeOffset? endDate;

        [ObservableProperty]
        private string jobDescription = string.Empty;

        [ObservableProperty]
        private string jobLocation = string.Empty;

        [ObservableProperty]
        private double availablePositions = 1;

        [ObservableProperty]
        private string photo = string.Empty;

        [ObservableProperty]
        private string salaryText = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? deadline;

        /// <summary>
        /// Saves the job to the database if the inputs are valid.
        /// </summary>
        [RelayCommand]
        public async Task SaveJobAsync()
        {
            if (this.sessionService?.LoggedInUser == null)
            {
                return;
            }

            int companyId = this.sessionService.LoggedInUser.CompanyId;

            int? salary = null;
            if (!string.IsNullOrWhiteSpace(this.SalaryText)
                && int.TryParse(this.SalaryText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedSalary))
            {
                salary = parsedSalary;
            }

            if (string.IsNullOrWhiteSpace(this.JobTitle))
            {
                this.OnSaveCompleted?.Invoke(false, "Job title is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.IndustryField))
            {
                this.OnSaveCompleted?.Invoke(false, "Industry field is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.JobType))
            {
                this.OnSaveCompleted?.Invoke(false, "Job type is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.ExperienceLevel))
            {
                this.OnSaveCompleted?.Invoke(false, "Experience level is required.");
                return;
            }

            if (!this.StartDate.HasValue || !this.EndDate.HasValue)
            {
                this.OnSaveCompleted?.Invoke(false, "Start date and end date are required.");
                return;
            }

            if (this.EndDate.Value.Date < this.StartDate.Value.Date)
            {
                this.OnSaveCompleted?.Invoke(false, "End date must be on or after start date.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.JobDescription))
            {
                this.OnSaveCompleted?.Invoke(false, "Job description is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.JobLocation))
            {
                this.OnSaveCompleted?.Invoke(false, "Job location is required.");
                return;
            }

            if (this.AvailablePositions < 1)
            {
                this.OnSaveCompleted?.Invoke(false, "Available positions must be at least 1.");
                return;
            }

            if (salary.HasValue && salary.Value < 0)
            {
                this.OnSaveCompleted?.Invoke(false, "Salary cannot be negative.");
                return;
            }

            var links = new List<(int SkillId, int RequiredPercentage)>();
            foreach (var row in this.SkillRows.Where(r => r.IsSelected))
            {
                if (!int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var pct)
                    && !int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out pct))
                {
                    this.OnSaveCompleted?.Invoke(false, $"Invalid percentage for skill \"{row.Skill.SkillName}\".");
                    return;
                }

                if (pct < 1 || pct > 100)
                {
                    this.OnSaveCompleted?.Invoke(false, $"Required percentage for \"{row.Skill.SkillName}\" must be between 1 and 100.");
                    return;
                }

                links.Add((row.Skill.SkillId, pct));
            }

            if (links.Count == 0)
            {
                this.OnSaveCompleted?.Invoke(false, "Select at least one required skill with a valid percentage (1–100).");
                return;
            }

            var job = new JobPosting
            {
                JobTitle = this.JobTitle.Trim(),
                IndustryField = this.IndustryField.Trim(),
                JobType = this.JobType.Trim(),
                ExperienceLevel = this.ExperienceLevel.Trim(),
                StartDate = this.StartDate?.DateTime.Date,
                EndDate = this.EndDate?.DateTime.Date,
                JobDescription = this.JobDescription.Trim(),
                JobLocation = this.JobLocation.Trim(),
                AvailablePositions = (int)this.AvailablePositions,
                Photo = string.IsNullOrWhiteSpace(this.Photo) ? null : this.Photo.Trim(),
                PostedAt = DateTime.Now,
                Salary = salary,
                AmountPayed = 0,
                Deadline = this.Deadline?.DateTime.Date,
            };

            try
            {
                var newId = await this.jobsService.AddJob(job, companyId, links);
                this.OnSaveCompleted?.Invoke(true, $"Job created successfully.");
            }
            catch (Exception ex)
            {
                this.OnSaveCompleted?.Invoke(false, ex.Message);
            }
        }
    }
}
