using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiSkillPickItem : ObservableObject
{
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private string requiredPercentText = "50";
    public TiSkillDto Skill { get; init; } = new();
}
