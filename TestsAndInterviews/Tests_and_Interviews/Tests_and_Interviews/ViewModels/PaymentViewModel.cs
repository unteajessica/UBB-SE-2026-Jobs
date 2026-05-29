using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Validators;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.ViewModels
{
    public partial class PaymentViewModel : ObservableObject
    {
        private const string DefaultJobTypeValue = "Full-Time";
        private const string DefaultExperienceLevelValue = "Senior";
        private const int MinimumValidPaymentAmount = 0;

        private const string MessageTitleInvalidAmount = "Invalid Amount";
        private const string MessageBodyInvalidAmount = "Please enter a valid numerical amount greater than 0.";
        private const string MessageTitleError = "Error";
        private const string MessageTitleSuccess = "Success";
        private const string MessageBodySuccessPrefix = "Payment of $";
        private const string MessageBodySuccessSuffix = " processed successfully. Emails dispatched!";

        private readonly IPaymentService paymentService;

        [ObservableProperty] private string cardHolderName = string.Empty;
        [ObservableProperty] private string cardNumber = string.Empty;
        [ObservableProperty] private string expDate = string.Empty;
        [ObservableProperty] private string cvv = string.Empty;
        [ObservableProperty] private string amountToPayText = string.Empty;

        [ObservableProperty] private string selectedJobType = DefaultJobTypeValue;
        [ObservableProperty] private string selectedExperienceLevel = DefaultExperienceLevelValue;

        public ObservableCollection<JobPaymentInfo> PaymentData { get; } = new ObservableCollection<JobPaymentInfo>();

        public Action<string, string>? ShowMessageAction { get; set; }
        public Action? CloseWindowAction { get; set; }
        public int CurrentJobId { get; set; }

        public PaymentViewModel(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
            LoadData(); // Load initially
        }

        partial void OnSelectedJobTypeChanged(string value) => LoadData();
        partial void OnSelectedExperienceLevelChanged(string value) => LoadData();

        private async void LoadData()
        {
            if (string.IsNullOrEmpty(SelectedJobType) || string.IsNullOrEmpty(SelectedExperienceLevel))
            {
                return;
            }

            PaymentData.Clear();
            var dataFromDatabase = await paymentService.GetPaidJobsInfo(SelectedJobType, SelectedExperienceLevel);

            foreach (var item in dataFromDatabase)
            {
                PaymentData.Add(item);
            }
        }

        [RelayCommand]
        private async Task Pay()
        {
            if (!int.TryParse(AmountToPayText, out int amountToPay) || amountToPay <= MinimumValidPaymentAmount)
            {
                ShowMessageAction?.Invoke(MessageTitleInvalidAmount, MessageBodyInvalidAmount);
                return;
            }

            string resultMessage = await paymentService.ProcessPaymentAsync(CurrentJobId, amountToPay, CardHolderName, CardNumber, ExpDate, Cvv);

            if (!string.IsNullOrEmpty(resultMessage))
            {
                ShowMessageAction?.Invoke(MessageTitleError, resultMessage);
            }
            else
            {
                ShowMessageAction?.Invoke(MessageTitleSuccess, $"{MessageBodySuccessPrefix}{amountToPay}{MessageBodySuccessSuffix}");
                LoadData();
                AmountToPayText = string.Empty;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseWindowAction?.Invoke();
        }
    }
}