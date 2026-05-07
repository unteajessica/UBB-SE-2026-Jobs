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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.LoadAllRolesAsync();

        var error = viewModel.GetErrorMessage();
        if (!string.IsNullOrEmpty(error))
        {
            var dialog = new ContentDialog { Title = "Error", Content = error, CloseButtonText = "OK", XamlRoot = XamlRoot };
            await dialog.ShowAsync();
            return;
        }

        var displayItems = new List<object>();
        foreach (var result in viewModel.GetRoleResults())
        {
            displayItems.Add(new
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

    private void RolesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (rolesList.SelectedItem is null) return;
        dynamic selected = rolesList.SelectedItem;
        RoleResult result = selected.Result;
        viewModel.GetResultForRole(result.JobRole);
        Frame.Navigate(typeof(CompatibilityDetailPage), result);
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }
}
