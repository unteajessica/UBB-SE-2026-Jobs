using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PussyCats.App.ViewModels.TI;

public class TiAnswerReviewItem
{
    public string QuestionText { get; set; } = string.Empty;
    public string UserAnswerDisplay { get; set; } = string.Empty;
    public string CorrectAnswerDisplay { get; set; } = string.Empty;
    public string EarnedScoreDisplay { get; set; } = string.Empty;
    public string MaxScoreDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Visibility ShowUserAnswer => Status == "Correct" ? Visibility.Collapsed : Visibility.Visible;

    public SolidColorBrush StatusBackground => Status switch
    {
        "Correct" => new SolidColorBrush(ColorHelper.FromArgb(255, 220, 252, 231)),
        "Partial" => new SolidColorBrush(ColorHelper.FromArgb(255, 254, 243, 199)),
        _ => new SolidColorBrush(ColorHelper.FromArgb(255, 254, 226, 226))
    };

    public SolidColorBrush StatusForeground => Status switch
    {
        "Correct" => new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74)),
        "Partial" => new SolidColorBrush(ColorHelper.FromArgb(255, 217, 119, 6)),
        _ => new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38))
    };
}
