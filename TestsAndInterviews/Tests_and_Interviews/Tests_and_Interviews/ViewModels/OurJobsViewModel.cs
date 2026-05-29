using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.ViewModels
{
    public partial class OurJobsViewModel : ObservableObject
    {
        private const string ErrorMessagePrefix = "Database error loading jobs: ";

        private readonly IJobsService jobsService;

        public Visibility JobsVisibility => Visibility.Visible;
        public Visibility BackButtonVisibility => Visibility.Collapsed;

        public ObservableCollection<JobPosting> Jobs { get; } = new ObservableCollection<JobPosting>();

        public OurJobsViewModel(IJobsService jobsService)
        {
            this.jobsService = jobsService;
        }

        public async Task LoadJobsAsync()
        {
            Jobs.Clear();
            try
            {
                var jobsFromDatabase = await jobsService.GetAllJobsAsync();
                foreach (var job in jobsFromDatabase)
                {
                    Jobs.Add(job);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"{ErrorMessagePrefix}{exception.Message}");
            }
        }

        public async Task ReloadJobsAsync()
        {
            await LoadJobsAsync();
        }
    }
}
