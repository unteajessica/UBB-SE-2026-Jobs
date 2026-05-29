using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats_App.Views.Auth;
using PussyCats_App.Views.Candidate;

namespace PussyCats_App;

public sealed partial class MainWindow : Window
{
    private bool suppressModeSelection;

    private static readonly HashSet<string> CandidatePages =
    [
        "UserRecommendationPage",
        "UserStatusPage",
        "UserProfilePage",
        "ProfileFormPage",
        "TestDashboardPage",
        "PersonalityTestPage",
        "CompatibilityOverviewPage",
        "DocumentsPage",
        "ExportCVPage",
        "ChatPage",
    ];

    private static readonly HashSet<string> CompanyPages =
    [
        "CompanyRecommendationPage",
        "CompanyStatusPage",
        "ChatPage",
    ];

    private static readonly HashSet<string> DeveloperPages =
    [
        "DeveloperPage",
    ];

    private static readonly HashSet<string> SharedPages = [];

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
        ["DeveloperPage"]             = typeof(Views.Developer.DeveloperPage),
        ["ChatPage"]                  = typeof(Views.ChatPage),
        ["LoginPage"]                 = typeof(LoginPage),
        ["RegisterPage"]              = typeof(RegisterPage),
    };

    public Frame NavigationFrame => contentFrame;

    public MainWindow()
    {
        InitializeComponent();
        contentFrame.Navigated += ContentFrame_Navigated;
        var session = App.Services.GetRequiredService<SessionContext>();
        if (session.IsAuthenticated)
        {
            ShowAuthenticatedShell();
        }
        else
        {
            ShowLogin();
        }
    }

    public void ShowAuthenticatedShell()
    {
        var session = App.Services.GetRequiredService<SessionContext>();
        if (!session.IsAuthenticated)
        {
            ShowLogin();
            return;
        }

        navView.IsPaneVisible = true;
        navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Auto;
        modeSelector.Visibility = navView.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;

        suppressModeSelection = true;
        modeSelector.SelectedIndex = session.Mode switch
        {
            AppMode.Company => 1,
            AppMode.Developer => 2,
            _ => 0,
        };
        suppressModeSelection = false;

        UpdateModeVisibility();
        var defaultPage = GetDefaultPage(session.Mode);
        NavigateTo(defaultPage);
        UpdateNavSelection(defaultPage);
    }

    public void ShowLogin()
    {
        navView.IsPaneVisible = false;
        navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        navView.SelectedItem = null;
        modeSelector.Visibility = Visibility.Collapsed;
        contentFrame.BackStack.Clear();
        contentFrame.Navigate(typeof(LoginPage));
    }

    public void ShowRegister()
    {
        navView.IsPaneVisible = false;
        navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        navView.SelectedItem = null;
        modeSelector.Visibility = Visibility.Collapsed;
        contentFrame.BackStack.Clear();
        contentFrame.Navigate(typeof(RegisterPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs eventArguments)
    {
        if (eventArguments.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            if (tag == "Logout")
            {
                App.Services.GetRequiredService<SessionContext>().SignOut();
                ShowLogin();
                return;
            }

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
        modeSelector.Header = "Mode";
        modeSelector.Visibility = Visibility.Visible;
    }

    private void NavView_PaneClosed(NavigationView sender, object args)
        => modeSelector.Visibility = Visibility.Collapsed;

    private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs eventArguments)
    {
        if (suppressModeSelection)
        {
            return;
        }

        if (modeSelector.SelectedItem is not ComboBoxItem { Tag: string tag })
        {
            return;
        }

        var session = App.Services.GetRequiredService<SessionContext>();
        if (!session.IsAuthenticated)
        {
            ShowLogin();
            return;
        }

        session.Mode = tag switch
        {
            "Company" => AppMode.Company,
            "Developer" => AppMode.Developer,
            _ => AppMode.Candidate,
        };

        if (session.Mode == AppMode.Company && session.CompanyId is null)
        {
            session.CompanyId = 1;
        }

        if (session.Mode == AppMode.Developer && session.DeveloperId is null)
        {
            session.DeveloperId = 1;
        }

        UpdateModeVisibility();

        var defaultPage = GetDefaultPage(session.Mode);
        NavigateTo(defaultPage);
        UpdateNavSelection(defaultPage);
    }

    private void UpdateModeVisibility()
    {
        var session = App.Services.GetRequiredService<SessionContext>();
        foreach (var item in navView.MenuItems)
        {
            if (item is not NavigationViewItem navigationViewItem || navigationViewItem.Tag is not string tag)
            {
                continue;
            }

            var visible = SharedPages.Contains(tag)
                || session.Mode == AppMode.Candidate && CandidatePages.Contains(tag)
                || session.Mode == AppMode.Company && CompanyPages.Contains(tag)
                || session.Mode == AppMode.Developer && DeveloperPages.Contains(tag);

            navigationViewItem.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void NavigateTo(string tag)
    {
        var session = App.Services.GetRequiredService<SessionContext>();
        if (!session.IsAuthenticated && tag != "LoginPage" && tag != "RegisterPage")
        {
            ShowLogin();
            return;
        }

        if (PageMap.TryGetValue(tag, out var pageType))
            contentFrame.Navigate(pageType);
    }

    private void UpdateNavSelection(string tag)
    {
        foreach (var item in navView.MenuItems)
        {
            if (item is NavigationViewItem navigationViewItem && navigationViewItem.Tag as string == tag)
            {
                navView.SelectedItem = navigationViewItem;
                return;
            }
        }

        foreach (var item in navView.FooterMenuItems)
        {
            if (item is NavigationViewItem navigationViewItem && navigationViewItem.Tag as string == tag)
            {
                navView.SelectedItem = navigationViewItem;
                return;
            }
        }
    }

    private static string GetDefaultPage(AppMode mode)
    {
        return mode switch
        {
            AppMode.Company => "CompanyRecommendationPage",
            AppMode.Developer => "DeveloperPage",
            _ => "UserRecommendationPage",
        };
    }
}
