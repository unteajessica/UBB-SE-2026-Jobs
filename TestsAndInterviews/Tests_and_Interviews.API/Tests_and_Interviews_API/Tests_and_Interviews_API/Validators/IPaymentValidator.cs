namespace Tests_and_Interviews_API.Validators
{
    public interface IPaymentValidator
    {
        string ValidatePaymentDetails(string cardHolderName, string cardNumber, string expirationDate, string cardVerificationValue);
    }
}