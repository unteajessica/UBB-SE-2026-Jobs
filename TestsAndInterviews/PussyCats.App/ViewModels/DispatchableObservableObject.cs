using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;

namespace PussyCats.App.ViewModels;

/// <summary>
/// ObservableObject that marshals every PropertyChanged notification to the UI
/// thread via UIDispatcher. Required in WinUI 3 because the native COM proxy for
/// INotifyPropertyChanged throws RPC_E_WRONG_THREAD when invoked off-thread.
/// </summary>
public abstract class DispatchableObservableObject : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs eventArguments)
    {
        UIDispatcher.Enqueue(() => base.OnPropertyChanged(eventArguments));
    }
}
