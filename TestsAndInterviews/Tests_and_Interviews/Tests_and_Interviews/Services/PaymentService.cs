// <copyright file="PaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Validators;

    public class PaymentService : IPaymentService
    {
        private const string DatabaseErrorMessagePrefix = "Database Error: ";
        private readonly IPaymentValidator validator;
        private readonly HttpClient http;

        public PaymentService(IPaymentValidator paymentValidator)
        {
            this.validator = paymentValidator;
            this.http = ApiClient.Http;
        }

        public PaymentService(IPaymentValidator paymentValidator, HttpClient httpClient)
        {
            this.validator = paymentValidator;
            this.http = httpClient ?? ApiClient.Http;
        }

        public async Task<string> ProcessPaymentAsync(int jobId, int amount, string name, string cardNum, string exp, string cvv)
        {
            string validationError = this.validator.ValidatePaymentDetails(name, cardNum, exp, cvv);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            try
            {
                HttpResponseMessage response = await this.http.PostAsJsonAsync(
                    $"payment/process/{jobId}?paymentAmount={amount}",
                    new { });
                response.EnsureSuccessStatusCode();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return $"{DatabaseErrorMessagePrefix}{exception.Message}";
            }
        }

        public async Task<List<JobPaymentInfo>> GetPaidJobsInfo(string jobType, string expLevel)
        {
            HttpResponseMessage response = await this.http.GetAsync(
                $"payment/paid?jobType={jobType}&experienceLevel={expLevel}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<JobPaymentInfo>();
            }

            response.EnsureSuccessStatusCode();
            List<JobPaymentInfoDto>? dtos = await response.Content.ReadFromJsonAsync<List<JobPaymentInfoDto>>();
            return dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<JobPaymentInfo>();
        }
    }
}