using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Views.Candidate;

public sealed partial class CompatibilityDetailPage : Page
{
    private readonly CompatibilityDetailViewModel viewModel;
    private const double InsufficientDataScore = -1;

    public CompatibilityDetailPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<CompatibilityDetailViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        if (eventArguments.Parameter is RoleResult result)
            viewModel.LoadResult(result);
    }

    private void Page_Loaded(object sender, RoutedEventArgs eventArguments)
    {
        roleNameLabel.Text = viewModel.GetRoleName();
        var score = viewModel.GetMatchScore();
        matchScoreLabel.Text = score == InsufficientDataScore
            ? "Score: Insufficient Data"
            : $"Match Score: {Math.Round(score, 1)}%";

        var suggestions = viewModel.GetSuggestions();
        if (suggestions is null || suggestions.Count == 0)
        {
            suggestionsList.Visibility  = Visibility.Collapsed;
            noSuggestionsLabel.Visibility = Visibility.Visible;
            return;
        }

        var items = new List<object>();
        foreach (var suggestion in suggestions)
        {
            items.Add(new
            {
                suggestion.SkillName,
                suggestion.GroupName,
                GainDisplay = $"Potential gain: +{Math.Round(suggestion.GainScore, 1)}%",
            });
        }

        suggestionsList.ItemsSource  = items;
        suggestionsList.Visibility   = Visibility.Visible;
        noSuggestionsLabel.Visibility = Visibility.Collapsed;
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }
}
