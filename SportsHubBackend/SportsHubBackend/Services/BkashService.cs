using SportsHubBackend.Model;
using SportsHubBackend.Services;
using System.Text;
using System.Text.Json;

namespace SportsHubBackend.Services
{

    public class BKashService : IBKashService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BKashService> _logger;
        private readonly HttpClient _httpClient;

        public BKashService(IConfiguration configuration, ILogger<BKashService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PaymentResponse> InitiatePaymentAsync(PaymentRequest request)
        {
            try
            {
                // Ensure OrderId is not empty
                if (string.IsNullOrWhiteSpace(request.OrderId))
                {
                    request.OrderId = $"Inv{new Random().Next(100000, 999999)}";
                }

                var appKey = _configuration["bKash:AppKey"] ?? string.Empty;
                var appSecret = _configuration["bKash:AppSecret"] ?? string.Empty;
                var username = _configuration["bKash:Username"] ?? string.Empty;
                var password = _configuration["bKash:Password"] ?? string.Empty;
                var baseUrl = _configuration["bKash:BaseUrl"] ?? "https://tokenized.sandbox.bka.sh/v1.2.0-beta";

                // First, get access token (PHP: getAuthToken())
                var token = await GetAccessTokenAsync(baseUrl, appKey, appSecret, username, password);
                if (string.IsNullOrEmpty(token))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Auth Failed",
                        Message = "Unable to authenticate with bKash API"
                    };
                }

                // Create payment request (PHP: createPaymentLink)
                // PHP: BKS_URL = 'https://tokenized.pay.bka.sh/v1.2.0-beta/tokenized/checkout'
                // PHP: BKS_URL . '/create' = baseUrl + "/tokenized/checkout/create"
                // But our baseUrl is already "https://tokenized.sandbox.bka.sh/v1.2.0-beta"
                // So we need: baseUrl + "/tokenized/checkout/create"
                var paymentUrl = $"{baseUrl}/tokenized/checkout/create";
                var paymentRequest = new
                {
                    mode = "0011",
                    amount = request.Amount.ToString("F2"),
                    payerReference = "test for api",
                    callbackURL = request.SuccessUrl,
                    currency = request.Currency,
                    intent = "sale",
                    merchantInvoiceNumber = request.MerchantInvoiceNumber ?? request.OrderId
                };

                var jsonContent = JsonSerializer.Serialize(paymentRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Headers: Content-Type:application/json, Authorization:token, X-APP-Key:appKey
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _httpClient.DefaultRequestHeaders.Add("X-APP-Key", appKey);

                var response = await _httpClient.PostAsync(paymentUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("bKash Payment Initiation Response: {Response}", responseContent);

                var apiResponse = JsonSerializer.Deserialize<BKashApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to parse bKash response",
                        Message = "Payment initiation failed"
                    };
                }

                // PHP: if ($response['statusCode'] == "0000")
                if (apiResponse.StatusCode == "0000" && !string.IsNullOrEmpty(apiResponse.BkashUrl))
                {
                    return new PaymentResponse
                    {
                        Success = true,
                        PaymentId = apiResponse.PaymentId,
                        PaymentUrl = apiResponse.BkashUrl,
                        Status = apiResponse.StatusCode,
                        StatusMessage = apiResponse.StatusMessage,
                        Message = "Payment session created successfully"
                    };
                }

                // PHP: else return statusMessage
                return new PaymentResponse
                {
                    Success = false,
                    Status = apiResponse.StatusCode,
                    ErrorMessage = apiResponse.StatusMessage,
                    Message = apiResponse.StatusMessage ?? "Payment initiation failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating bKash payment");
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponse> ConfirmPaymentAsync(string paymentId)
        {
            try
            {
                var appKey = _configuration["bKash:AppKey"]!;
                var appSecret = _configuration["bKash:AppSecret"]!;
                var username = _configuration["bKash:Username"]!;
                var password = _configuration["bKash:Password"]!;
                var baseUrl = _configuration["bKash:BaseUrl"]
                              ?? "https://tokenized.sandbox.bka.sh/v1.2.0-beta";

                // 1️⃣ Get Access Token
                var token = await GetAccessTokenAsync(baseUrl, appKey, appSecret, username, password);
                if (string.IsNullOrEmpty(token))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "bKash authentication failed"
                    };
                }

                // 2️⃣ Execute Payment
                var executeUrl = $"{baseUrl}/tokenized/checkout/execute";

                var payload = new
                {
                    paymentID = paymentId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _httpClient.DefaultRequestHeaders.Add("X-APP-Key", appKey);

                var response = await _httpClient.PostAsync(executeUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("bKash Execute Response: {Response}", responseJson);

                var bkashResponse = JsonSerializer.Deserialize<BKashApiResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // 3️⃣ Validate Payment
                if (bkashResponse == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid response from bKash"
                    };
                }

                if (bkashResponse.StatusCode != "0000")
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Status = bkashResponse.StatusCode,
                        ErrorMessage = bkashResponse.StatusMessage
                    };
                }

                // 4️⃣ (IMPORTANT) Update Booking / Order Status
                // Example:
                // await _bookingService.MarkAsPaidAsync(bkashResponse.PaymentId);

                // 5️⃣ Final Success Response
                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = bkashResponse.PaymentId,
                    TrxId = bkashResponse.TrxId,
                    Status = bkashResponse.StatusCode,
                    Message = "Payment confirmed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming bKash payment");

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Payment confirmation failed"
                };
            }
        }

        public async Task<PaymentResponse> QueryPaymentAsync(string paymentId)
        {
            try
            {
                var appKey = _configuration["bKash:AppKey"] ?? string.Empty;
                var appSecret = _configuration["bKash:AppSecret"] ?? string.Empty;
                var username = _configuration["bKash:Username"] ?? string.Empty;
                var password = _configuration["bKash:Password"] ?? string.Empty;
                var baseUrl = _configuration["bKash:BaseUrl"] ?? "https://tokenized.sandbox.bka.sh/v1.2.0-beta";

                var token = await GetAccessTokenAsync(baseUrl, appKey, appSecret, username, password);
                if (string.IsNullOrEmpty(token))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Failed to get access token"
                    };
                }

                var queryUrl = $"{baseUrl}/tokenized/checkout/payment/query/{paymentId}";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _httpClient.DefaultRequestHeaders.Add("X-App-Key", appKey);

                var response = await _httpClient.GetAsync(queryUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("bKash Payment Query Response: {Response}", responseContent);

                var apiResponse = JsonSerializer.Deserialize<BKashApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.StatusCode == "0000")
                {
                    return new PaymentResponse
                    {
                        Success = true,
                        PaymentId = apiResponse.PaymentId,
                        Status = apiResponse.StatusCode,
                        StatusMessage = apiResponse.StatusMessage
                    };
                }

                return new PaymentResponse
                {
                    Success = false,
                    Status = apiResponse?.StatusCode,
                    ErrorMessage = apiResponse?.ErrorMessage ?? apiResponse?.StatusMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying bKash payment");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> VerifyPaymentAsync(PaymentCallback callback)
        {
            try
            {
                if (string.IsNullOrEmpty(callback.PaymentId))
                {
                    return false;
                }

                var queryResponse = await QueryPaymentAsync(callback.PaymentId);
                return queryResponse.Success && queryResponse.Status == "0000";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying bKash payment");
                return false;
            }
        }

        private async Task<string?> GetAccessTokenAsync(string baseUrl, string appKey, string appSecret, string username, string password)
        {
            try
            {
                // PHP: getAuthToken() - BKS_URL . '/token/grant'
                // PHP: BKS_URL = 'https://tokenized.pay.bka.sh/v1.2.0-beta/tokenized/checkout'
                // So: baseUrl + "/tokenized/checkout/token/grant"
                var tokenUrl = $"{baseUrl}/tokenized/checkout/token/grant";

                // PHP: json_encode(['app_key' => BKS_KEY, 'app_secret' => BKS_SEC])
                var requestData = new
                {
                    app_key = appKey,
                    app_secret = appSecret
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // PHP Headers: Content-Type:application/json, username:, password:
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("username", username);
                _httpClient.DefaultRequestHeaders.Add("password", password);

                var response = await _httpClient.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("bKash Token Response: {Response}", responseContent);

                // PHP: if ($response['statusCode'] == "0000") return $response['id_token'];
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (tokenResponse.TryGetProperty("statusCode", out var statusCode) &&
                    statusCode.GetString() == "0000" &&
                    tokenResponse.TryGetProperty("id_token", out var idToken))
                {
                    return idToken.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bKash access token");
                return null;
            }
        }
    }
}