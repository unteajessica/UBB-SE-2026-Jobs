using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PussyCats.App.ViewModels.TI;

public class TiOptionViewModel : INotifyPropertyChanged
{
    private bool isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Text { get; set; } = string.Empty;
    public int Index { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Action? OnSelectionChanged { get; set; }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Notify();
            OnSelectionChanged?.Invoke();
        }
    }

    private void Notify([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
