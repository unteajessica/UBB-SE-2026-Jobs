using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Controls;

public sealed partial class SkillTestCardControl : UserControl
{
    private readonly SkillTestCardViewModel viewModel;
    private const int BadgeIconRasterizeSize = 100;

    /// <summary>Raised with the TI test id when the user wants to take/continue the test.</summary>
    public event EventHandler<int>? TakeTestRequested;

    public SkillTestCardControl(SkillTestCardViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        LoadCard();
    }

    private void LoadCard()
    {
        TestNameText.Text = viewModel.Title.ToUpper();
        ScoreText.Text = viewModel.ScoreText;
        DateText.Text = string.IsNullOrEmpty(viewModel.DateText)
            ? viewModel.Status
            : $"{viewModel.Status} · {viewModel.DateText}";

        if (viewModel.Badge is not null && !string.IsNullOrEmpty(viewModel.Badge.IconPath))
        {
            var path = viewModel.Badge.IconPath;
            if (!path.StartsWith("ms-appx:///", StringComparison.Ordinal))
                path = $"ms-appx:///{path.TrimStart('/')}";

            BadgeIcon.Source = new SvgImageSource(new Uri(path))
            {
                RasterizePixelWidth = BadgeIconRasterizeSize,
                RasterizePixelHeight = BadgeIconRasterizeSize,
            };
        }

        ActionButton.Content = viewModel.ActionLabel;
        ActionButton.IsEnabled = viewModel.CanTakeTest;
        ActionButton.Opacity = viewModel.CanTakeTest ? 1.0 : 0.4;
    }

    private void ActionButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (viewModel.CanTakeTest)
            TakeTestRequested?.Invoke(this, viewModel.TestId);
    }
}
