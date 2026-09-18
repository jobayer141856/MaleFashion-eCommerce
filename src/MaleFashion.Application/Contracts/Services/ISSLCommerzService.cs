using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Contracts.Services
{
    public class SSLCommerzInitRequest
    {
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string TransactionId { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;

        // Customer details
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerPostCode { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = "Bangladesh";
        public string CustomerPhone { get; set; } = string.Empty;

        // Shipping details
        public string ShippingName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = "Bangladesh";
        public int TotalItems { get; set; } = 1;
    }

    public class SSLCommerzInitResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? FailedReason { get; set; }
        public string? SessionKey { get; set; }
        public string? GatewayPageURL { get; set; }
        public bool IsSuccess => string.Equals(Status, "SUCCESS", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(GatewayPageURL);
    }

    public class SSLCommerzValidationResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? TranId { get; set; }
        public string? ValId { get; set; }
        public decimal Amount { get; set; }
        public string? BankTranId { get; set; }
        public string? CardType { get; set; }
        public string? Currency { get; set; }
        public string? Error { get; set; }
        public bool IsValid => string.Equals(Status, "VALID", StringComparison.OrdinalIgnoreCase) || string.Equals(Status, "VALIDATED", StringComparison.OrdinalIgnoreCase);
    }

    public interface ISSLCommerzService
    {
        Task<SSLCommerzInitResponse> InitiatePaymentAsync(SSLCommerzInitRequest request, CancellationToken cancellationToken = default);
        Task<SSLCommerzValidationResponse> ValidatePaymentAsync(string valId, CancellationToken cancellationToken = default);
    }
}

