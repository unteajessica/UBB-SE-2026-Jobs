using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Views.Candidate;

public sealed partial class CompatibilityOverviewPage : Page
{
    private readonly CompatibilityOverviewViewModel viewModel;
    private const double InsufficientDataScore = -1;

    public CompatibilityOverviewPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<CompatibilityOverviewViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.LoadAllRolesAsync();

        var error = viewModel.GetErrorMessage();
        if (!string.IsNullOrEmpty(error))
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = error,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
            return;
        }

        var displayItems = new List<CompatibilityDisplayItem>();
        foreach (var result in viewModel.GetRoleResults())
        {
            displayItems.Add(new CompatibilityDisplayItem
            {
                Result = result,
                DisplayName = ViewModelSupport.FormatJobRole(result.JobRole),
                DisplayScore = result.MatchScore == InsufficientDataScore ? 0 : result.MatchScore,
                DisplayPercentage = result.MatchScore == InsufficientDataScore
                    ? "Insufficient Data"
                    : Math.Round(result.MatchScore, 1) + "%",
            });
        }

        rolesList.ItemsSource = displayItems;
    }

    private void RolesList_SelectionChanged(object sender, SelectionChangedEventArgs eventArguments)
    {
        if (rolesList.SelectedItem is not CompatibilityDisplayItem selected) return;
        Frame.Navigate(typeof(CompatibilityDetailPage), selected.Result);
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }
}
