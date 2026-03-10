using SportsHubBackend.Model;

namespace SportsHubBackend.Services
{
    public interface IBKashService
    {
        Task<PaymentResponse> InitiatePaymentAsync(PaymentRequest request);
        Task<PaymentResponse> ConfirmPaymentAsync(string paymentId);
        Task<PaymentResponse> QueryPaymentAsync(string paymentId);
        Task<bool> VerifyPaymentAsync(PaymentCallback callback);
    }
}