using System.Text.Json.Serialization;

namespace SportsHubBackend.Model
{
    // Used when payment gateway sends callback data
    public class PaymentCallback
    {
        [JsonPropertyName("paymentID")]
        public string? PaymentId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("transactionStatus")]
        public string? TransactionStatus { get; set; }

        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("merchantInvoiceNumber")]
        public string? MerchantInvoiceNumber { get; set; }

        [JsonPropertyName("trxID")]
        public string? TrxId { get; set; }
    }

    // Request object to initiate a payment
    public class PaymentRequest
    {
        public int? ProductId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string? OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;

        public string? MerchantInvoiceNumber { get; set; }
    }

    // Response object for API to send back to frontend
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Status { get; set; }
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public string? TrxId { get; set; }
    }

    // Response from BKash API
    public class BKashApiResponse
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; set; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; set; }

        [JsonPropertyName("paymentID")]
        public string? PaymentId { get; set; }

        [JsonPropertyName("bkashURL")]
        public string? BkashUrl { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("trxID")]
        public string? TrxId { get; set; }
    }
}