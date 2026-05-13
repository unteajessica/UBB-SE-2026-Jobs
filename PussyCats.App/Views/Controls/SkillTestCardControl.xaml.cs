using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PussyCats.App.ViewModels;
using PussyCats_App.Services.SkillTestService;

namespace PussyCats_App.Views.Controls;

public sealed partial class SkillTestCardControl : UserControl
{
    private readonly SkillTestCardViewModel viewModel;
    private const int BadgeIconRasterizeSize = 100;

    public SkillTestCardControl(SkillTestCardViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        LoadCard();
    }

    private void LoadCard()
    {
        TestNameText.Text = (viewModel.SkillTest.Name?.ToUpper() ?? "UNKNOWN") + " TEST";
        ScoreText.Text = $"SCORE: {viewModel.SkillTest.Score}%";
        DateText.Text = SkillTestService.AchievedDateFormatted(viewModel.SkillTest);

        if (viewModel.Badge is not null && !string.IsNullOrEmpty(viewModel.Badge.IconPath))
        {
            var path = viewModel.Badge.IconPath;
            if (!path.StartsWith("ms-appx:///", StringComparison.Ordinal))
                path = $"ms-appx:///{path.TrimStart('/')}";

            var source = new SvgImageSource(new Uri(path))
            {
                RasterizePixelWidth = BadgeIconRasterizeSize,
                RasterizePixelHeight = BadgeIconRasterizeSize,
            };
            BadgeIcon.Source = source;
        }

        RetakeButton.IsEnabled = viewModel.IsRetakeEnabled;
        RetakeButton.Opacity = viewModel.IsRetakeEnabled ? 1.0 : 0.4;
    }

    private async void RetakeButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.RetakeCommand.ExecuteAsync(null);
        LoadCard();
    }
}
