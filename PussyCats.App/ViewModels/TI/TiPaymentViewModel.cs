using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiPaymentViewModel : DispatchableObservableObject
{
    private readonly ITiPaymentService paymentService;

    [ObservableProperty] private int jobId;
    [ObservableProperty] private string jobTitle = string.Empty;
    [ObservableProperty] private string cardHolderName = string.Empty;
    [ObservableProperty] private string cardNumber = string.Empty;
    [ObservableProperty] private string expDate = string.Empty;
    [ObservableProperty] private string cvv = string.Empty;
    [ObservableProperty] private string amountText = string.Empty;
    [ObservableProperty] private string selectedJobType = "Full-Time";
    [ObservableProperty] private string selectedExperienceLevel = "Senior";
    [ObservableProperty] private bool isProcessing;
    [ObservableProperty] private string resultMessage = string.Empty;
    [ObservableProperty] private bool paymentSucceeded;

    public ObservableCollection<TiJobPaymentInfoDto> PaymentData { get; } = new();

    public TiPaymentViewModel(ITiPaymentService paymentService)
    {
        this.paymentService = paymentService;
    }

    public async Task LoadDataAsync()
    {
        var data = await paymentService.GetPaidJobsInfoAsync(SelectedJobType, SelectedExperienceLevel);
        PaymentData.Clear();
        foreach (var d in data) PaymentData.Add(d);
    }

    [RelayCommand]
    public async Task PayAsync()
    {
        if (!decimal.TryParse(AmountText, out decimal amount) || amount <= 0)
        {
            ResultMessage = "Enter a valid positive amount.";
            return;
        }

        IsProcessing = true;
        ResultMessage = string.Empty;
        var error = await paymentService.ProcessPaymentAsync(JobId, amount, CardHolderName, CardNumber, ExpDate, Cvv);
        IsProcessing = false;

        if (string.IsNullOrEmpty(error))
        {
            PaymentSucceeded = true;
            ResultMessage = "Payment successful!";
        }
        else
        {
            ResultMessage = $"Payment failed: {error}";
        }
    }
}
