using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Validators
{
    public class PaymentValidator : IPaymentValidator
    {
        private const int CardNumberValidLength = 15;
        private const int CardVerificationValueValidLength = 3;
        private const int ExpirationDateValidNumberOfParts = 2;
        private const string ExpirationDateSeparatorString = "/";
        private const char ExpirationDateSeparatorCharacter = '/';
        private const int MinimumMonth = 1;
        private const int MaximumMonth = 12;
        private const int CenturyYearOffset = 2000;

        public string ValidatePaymentDetails(string cardHolderName, string cardNumber, string expirationDate, string cardVerificationValue)
        {
            if (string.IsNullOrWhiteSpace(cardHolderName))
            {
                return "Card Holder Name is required.";
            }

            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < CardNumberValidLength)
            {
                return "Please enter a valid Card Number.";
            }

            if (string.IsNullOrWhiteSpace(expirationDate) || !expirationDate.Contains(ExpirationDateSeparatorString))
            {
                return "Expiration Date must be in MM/YY format.";
            }

            if (string.IsNullOrWhiteSpace(cardVerificationValue) || cardVerificationValue.Length < CardVerificationValueValidLength)
            {
                return "Please enter a valid CVV.";
            }

            var expirationDateParts = expirationDate.Split(ExpirationDateSeparatorCharacter);
            if (expirationDateParts.Length != ExpirationDateValidNumberOfParts ||
                !int.TryParse(expirationDateParts[0], out int expirationMonth) ||
                !int.TryParse(expirationDateParts[1], out int expirationYear))
            {
                return "Expiration Date must contain valid numbers (MM/YY).";
            }

            if (expirationMonth < MinimumMonth || expirationMonth > MaximumMonth)
            {
                return "Invalid expiration month. Must be between 01 and 12.";
            }

            // Convert "YY" to "YYYY"
            expirationYear += CenturyYearOffset;

            // Check if the card is expired
            DateTime currentDate = DateTime.Now;
            if (expirationYear < currentDate.Year || (expirationYear == currentDate.Year && expirationMonth < currentDate.Month))
            {
                return "This card has expired. Please use a valid card.";
            }

            return string.Empty;
        }
    }
}