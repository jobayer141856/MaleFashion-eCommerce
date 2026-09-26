using MaleFashion.Application.Contracts.Services;
using MaleFashion.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Services
{
    public class SSLCommerzService : ISSLCommerzService
    {
        private readonly HttpClient _httpClient;
        private readonly SSLCommerzSettings _settings;
        private readonly ILogger<SSLCommerzService> _logger;

        public SSLCommerzService(
            HttpClient httpClient,
            IOptions<SSLCommerzSettings> settings,
            ILogger<SSLCommerzService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<SSLCommerzInitResponse> InitiatePaymentAsync(SSLCommerzInitRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _settings.BaseUrl.TrimEnd('/');
                var initUrl = $"{baseUrl}/gwprocess/v4/api.php";

                var postData = new Dictionary<string, string>
                {
                    { "store_id", _settings.StoreId },
                    { "store_passwd", _settings.StorePassword },
                    { "total_amount", request.TotalAmount.ToString("F2", CultureInfo.InvariantCulture) },
                    { "currency", string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency },
                    { "tran_id", request.TransactionId },
                    { "success_url", request.SuccessUrl },
                    { "fail_url", request.FailUrl },
                    { "cancel_url", request.CancelUrl },
                    { "ipn_url", request.IpnUrl },

                    // Customer Information
                    { "cus_name", string.IsNullOrWhiteSpace(request.CustomerName) ? "Customer" : request.CustomerName },
                    { "cus_email", string.IsNullOrWhiteSpace(request.CustomerEmail) ? "customer@example.com" : request.CustomerEmail },
                    { "cus_add1", string.IsNullOrWhiteSpace(request.CustomerAddress) ? "Dhaka" : request.CustomerAddress },
                    { "cus_city", string.IsNullOrWhiteSpace(request.CustomerCity) ? "Dhaka" : request.CustomerCity },
                    { "cus_postcode", string.IsNullOrWhiteSpace(request.CustomerPostCode) ? "1000" : request.CustomerPostCode },
                    { "cus_country", string.IsNullOrWhiteSpace(request.CustomerCountry) ? "Bangladesh" : request.CustomerCountry },
                    { "cus_phone", string.IsNullOrWhiteSpace(request.CustomerPhone) ? "01700000000" : request.CustomerPhone },

                    // Shipping Information
                    { "shipping_method", "Courier" },
                    { "num_of_item", Math.Max(1, request.TotalItems).ToString() },
                    { "ship_name", string.IsNullOrWhiteSpace(request.ShippingName) ? "Recipient" : request.ShippingName },
                    { "ship_add1", string.IsNullOrWhiteSpace(request.ShippingAddress) ? "Dhaka" : request.ShippingAddress },
                    { "ship_city", string.IsNullOrWhiteSpace(request.ShippingCity) ? "Dhaka" : request.ShippingCity },
                    { "ship_postcode", string.IsNullOrWhiteSpace(request.ShippingPostCode) ? "1000" : request.ShippingPostCode },
                    { "ship_country", string.IsNullOrWhiteSpace(request.ShippingCountry) ? "Bangladesh" : request.ShippingCountry },

                    // Product Information
                    { "product_name", "Male Fashion Apparel" },
                    { "product_category", "Fashion & Lifestyle" },
                    { "product_profile", "general" }
                };

                using var formContent = new FormUrlEncodedContent(postData);
                var response = await _httpClient.PostAsync(initUrl, formContent, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation("SSLCommerz Init Response: {Response}", responseString);

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                var result = new SSLCommerzInitResponse();

                if (root.TryGetProperty("status", out var statusProp))
                {
                    result.Status = statusProp.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("failedreason", out var failProp))
                {
                    result.FailedReason = failProp.GetString();
                }

                if (root.TryGetProperty("sessionkey", out var sessionProp))
                {
                    result.SessionKey = sessionProp.GetString();
                }

                if (root.TryGetProperty("GatewayPageURL", out var gwProp))
                {
                    result.GatewayPageURL = gwProp.GetString();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz payment initiation failed for Transaction ID: {TransactionId}", request.TransactionId);
                return new SSLCommerzInitResponse
                {
                    Status = "FAILED",
                    FailedReason = ex.Message
                };
            }
        }

        public async Task<SSLCommerzValidationResponse> ValidatePaymentAsync(string valId, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _settings.BaseUrl.TrimEnd('/');
                var validationUrl = $"{baseUrl}/validator/api/validationserverAPI.php?val_id={Uri.EscapeDataString(valId)}&store_id={Uri.EscapeDataString(_settings.StoreId)}&store_passwd={Uri.EscapeDataString(_settings.StorePassword)}&v=1&format=json";

                var response = await _httpClient.GetAsync(validationUrl, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation("SSLCommerz Validation Response: {Response}", responseString);

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                var result = new SSLCommerzValidationResponse();

                if (root.TryGetProperty("status", out var statusProp))
                    result.Status = statusProp.GetString() ?? string.Empty;

                if (root.TryGetProperty("tran_id", out var tranProp))
                    result.TranId = tranProp.GetString();

                if (root.TryGetProperty("val_id", out var valIdProp))
                    result.ValId = valIdProp.GetString();

                if (root.TryGetProperty("amount", out var amountProp))
                {
                    if (decimal.TryParse(amountProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
                    {
                        result.Amount = amt;
                    }
                }

                if (root.TryGetProperty("bank_tran_id", out var bankProp))
                    result.BankTranId = bankProp.GetString();

                if (root.TryGetProperty("card_type", out var cardProp))
                    result.CardType = cardProp.GetString();

                if (root.TryGetProperty("currency", out var currProp))
                    result.Currency = currProp.GetString();

                if (root.TryGetProperty("error", out var errProp))
                    result.Error = errProp.GetString();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz payment validation failed for ValId: {ValId}", valId);
                return new SSLCommerzValidationResponse
                {
                    Status = "FAILED",
                    Error = ex.Message
                };
            }
        }
    }
}

