using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats_App.Views.Candidate;

namespace PussyCats_App;

public sealed partial class MainWindow : Window
{
    private static readonly Dictionary<string, Type> PageMap = new()
    {
        ["UserRecommendationPage"]    = typeof(UserRecommendationPage),
        ["UserStatusPage"]            = typeof(UserStatusPage),
        ["UserProfilePage"]           = typeof(UserProfilePage),
        ["ProfileFormPage"]           = typeof(ProfileFormPage),
        ["TestDashboardPage"]         = typeof(TestDashboardPage),
        ["PersonalityTestPage"]       = typeof(PersonalityTestPage),
        ["CompatibilityOverviewPage"] = typeof(CompatibilityOverviewPage),
        ["DocumentsPage"]             = typeof(DocumentsPage),
        ["ExportCVPage"]              = typeof(ExportCVPage),
        ["PreferencesPage"]           = typeof(PreferencesPage),
        ["CompanyRecommendationPage"] = typeof(Views.Company.CompanyRecommendationPage),
        ["CompanyStatusPage"]         = typeof(Views.Company.CompanyStatusPage),
        ["ChatPage"]                  = typeof(Views.ChatPage),
    };

    public Frame NavigationFrame => contentFrame;

    public MainWindow()
    {
        InitializeComponent();
        contentFrame.Navigated += ContentFrame_Navigated;
        contentFrame.Navigate(typeof(UserRecommendationPage));
        UpdateNavSelection("UserRecommendationPage");
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs eventArguments)
    {
        if (eventArguments.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs eventArguments)
    {
        if (contentFrame.CanGoBack)
            contentFrame.GoBack();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs eventArguments)
    {
        navView.IsBackEnabled = contentFrame.CanGoBack;
    }

    private void NavView_PaneOpened(NavigationView sender, object args)
    {
        modeToggle.Header = "Company mode";
        modeToggle.Visibility = Visibility.Visible;
    }

    private void NavView_PaneClosed(NavigationView sender, object args)
        => modeToggle.Visibility = Visibility.Collapsed;

    private void ModeToggle_Toggled(object sender, RoutedEventArgs eventArguments)
    {
        var session = App.Services.GetRequiredService<SessionContext>();
        session.Mode = modeToggle.IsOn ? AppMode.Company : AppMode.Candidate;
        UpdateModeVisibility();

        var defaultPage = modeToggle.IsOn ? "CompanyRecommendationPage" : "UserRecommendationPage";
        NavigateTo(defaultPage);
        UpdateNavSelection(defaultPage);
    }

    private void UpdateModeVisibility()
    {
        var isCompany = modeToggle.IsOn;
        navReviewApplicants.Visibility = isCompany ? Visibility.Visible : Visibility.Collapsed;
        navApplicantStatus.Visibility  = isCompany ? Visibility.Visible : Visibility.Collapsed;
    }

    private void NavigateTo(string tag)
    {
        if (PageMap.TryGetValue(tag, out var pageType))
            contentFrame.Navigate(pageType);
    }

    private void UpdateNavSelection(string tag)
    {
        foreach (var item in navView.MenuItems)
        {
            if (item is NavigationViewItem nvi && nvi.Tag as string == tag)
            {
                navView.SelectedItem = nvi;
                return;
            }
        }

        foreach (var item in navView.FooterMenuItems)
        {
            if (item is NavigationViewItem nvi && nvi.Tag as string == tag)
            {
                navView.SelectedItem = nvi;
                return;
            }
        }
    }
}
