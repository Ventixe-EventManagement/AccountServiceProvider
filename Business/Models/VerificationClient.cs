namespace Business.Models;

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Business.Interfaces;

/// <summary>
/// Handles communication with the external VerificationServiceProvider
/// for sending and verifying email verification codes.
/// </summary>
/// <remarks>
/// Initializes the verification client with HttpClient and config.
/// </remarks>
public class VerificationClient(HttpClient httpClient, ILogger<VerificationClient> logger, IConfiguration config) : IVerificationClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<VerificationClient> _logger = logger;
    private readonly string _baseUrl = config["VerificationServiceUrl"]
            ?? throw new InvalidOperationException("Missing VerificationServiceUrl configuration.");

    /// <summary>
    /// Sends a verification code to the specified email address.
    /// </summary>
    public async Task SendVerificationCodeAsync(string email)
    {
        var payload = new { Email = email };
        var url = $"{_baseUrl}/verification/send";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Verification code sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification code to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Verifies the provided code for the given email.
    /// </summary>
    public async Task<bool> VerifyCodeAsync(string email, string code)
    {
        var payload = new { Email = email, Code = code };
        var url = $"{_baseUrl}/verification/verify";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Verification failed ({StatusCode}): {Body}", response.StatusCode, json);
                return false;
            }

            var result = JsonSerializer.Deserialize<VerificationResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Succeeded == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error verifying code for {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// DTO used for deserializing the verification response from the service.
    /// </summary>
    private class VerificationResponse
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
