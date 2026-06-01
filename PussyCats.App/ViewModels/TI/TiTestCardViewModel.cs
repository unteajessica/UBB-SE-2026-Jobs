using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PussyCats.App.ViewModels.TI;

public class TiTestCardViewModel : INotifyPropertyChanged
{
    private bool isSelected;
    private bool isHovered;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string QuestionTypeLabel { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Notify();
            Notify(nameof(CardBorderThickness));
            Notify(nameof(CardBorderBrush));
        }
    }

    public bool IsHovered
    {
        get => isHovered;
        set
        {
            isHovered = value;
            Notify();
            Notify(nameof(CardBorderThickness));
            Notify(nameof(CardBorderBrush));
        }
    }

    public Thickness CardBorderThickness =>
        IsSelected || IsHovered ? new Thickness(2.5) : new Thickness(1);

    public SolidColorBrush CardBorderBrush =>
        IsSelected
            ? new SolidColorBrush(ColorHelper.FromArgb(255, 132, 148, 255))
            : IsHovered
                ? new SolidColorBrush(ColorHelper.FromArgb(255, 30, 30, 30))
                : new SolidColorBrush(ColorHelper.FromArgb(255, 232, 228, 255));

    private void Notify([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
