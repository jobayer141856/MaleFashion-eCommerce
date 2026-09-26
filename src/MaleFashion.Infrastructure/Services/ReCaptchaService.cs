using MaleFashion.Application.Contracts.Services;
using MaleFashion.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Services
{
    public class ReCaptchaService : IRecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly ReCaptchaSettings _settings;
        private readonly ILogger<ReCaptchaService> _logger;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReCaptchaService(
            HttpClient httpClient,
            IOptions<ReCaptchaSettings> settings,
            ILogger<ReCaptchaService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<RecaptchaVerifyResponse> VerifyTokenAsync(
            string? token, 
            string? expectedAction = null, 
            CancellationToken cancellationToken = default)
        {
            // Bypass verification if disabled or if secret key is empty in development
            if (!_settings.IsEnabled || string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _logger.LogDebug("reCAPTCHA verification is disabled or SecretKey is not configured. Bypassing verification.");
                return new RecaptchaVerifyResponse
                {
                    Success = true,
                    Score = 1.0m,
                    Action = expectedAction
                };
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("reCAPTCHA token is empty for expected action '{Action}'.", expectedAction);
                return new RecaptchaVerifyResponse
                {
                    Success = false,
                    ErrorMessage = "Verification token is missing. Please try again."
                };
            }

            try
            {
                var postData = new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token
                };

                using var requestContent = new FormUrlEncodedContent(postData);
                using var response = await _httpClient.PostAsync(_settings.VerifyUrl, requestContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("reCAPTCHA verification request failed with status code {StatusCode}.", response.StatusCode);
                    return new RecaptchaVerifyResponse
                    {
                        Success = false,
                        ErrorMessage = "Unable to connect to reCAPTCHA verification service."
                    };
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var verifyResult = JsonSerializer.Deserialize<GoogleSiteVerifyApiResponse>(json, SerializerOptions);

                if (verifyResult == null || !verifyResult.Success)
                {
                    var errorCodes = verifyResult?.ErrorCodes != null ? string.Join(", ", verifyResult.ErrorCodes) : "None";
                    _logger.LogWarning("reCAPTCHA validation failed. Error codes: {ErrorCodes}", errorCodes);
                    return new RecaptchaVerifyResponse
                    {
                        Success = false,
                        Score = verifyResult?.Score ?? 0m,
                        Action = verifyResult?.Action,
                        ErrorCodes = verifyResult?.ErrorCodes,
                        ErrorMessage = "Security verification failed."
                    };
                }

                // Verify action if an expected action was provided
                if (!string.IsNullOrWhiteSpace(expectedAction) && 
                    !string.Equals(verifyResult.Action, expectedAction, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "reCAPTCHA action mismatch: expected '{ExpectedAction}', received '{ActualAction}'.",
                        expectedAction,
                        verifyResult.Action);

                    return new RecaptchaVerifyResponse
                    {
                        Success = false,
                        Score = verifyResult.Score,
                        Action = verifyResult.Action,
                        ErrorMessage = "Security verification action mismatch."
                    };
                }

                // Verify score meets minimum threshold
                if (verifyResult.Score < _settings.MinimumScore)
                {
                    _logger.LogWarning(
                        "reCAPTCHA score {Score} is below threshold {Threshold} for action '{Action}'.",
                        verifyResult.Score,
                        _settings.MinimumScore,
                        expectedAction);

                    return new RecaptchaVerifyResponse
                    {
                        Success = false,
                        Score = verifyResult.Score,
                        Action = verifyResult.Action,
                        ErrorMessage = "Suspicious activity detected. Please try again."
                    };
                }

                _logger.LogInformation(
                    "reCAPTCHA verified successfully for action '{Action}' with score {Score}.",
                    verifyResult.Action,
                    verifyResult.Score);

                return new RecaptchaVerifyResponse
                {
                    Success = true,
                    Score = verifyResult.Score,
                    Action = verifyResult.Action,
                    ChallengeTs = verifyResult.ChallengeTs,
                    Hostname = verifyResult.Hostname
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during reCAPTCHA verification for action '{Action}'.", expectedAction);
                return new RecaptchaVerifyResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while verifying security challenge."
                };
            }
        }

        private sealed class GoogleSiteVerifyApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public decimal Score { get; set; }

            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("challenge_ts")]
            public DateTime? ChallengeTs { get; set; }

            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }

            [JsonPropertyName("error-codes")]
            public List<string>? ErrorCodes { get; set; }
        }
    }
}

