namespace Business.Models;

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Business.Interfaces;

public class VerificationClient : IVerificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificationClient> _logger;
    private readonly string _baseUrl;

    public VerificationClient(HttpClient httpClient, ILogger<VerificationClient> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = config["VerificationServiceUrl"]
            ?? throw new InvalidOperationException("Missing VerificationServiceUrl config.");
    }

    public async Task SendVerificationCodeAsync(string email)
    {
        var payload = new { Email = email };
        var url = $"{_baseUrl}/verification/send";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("✅ Verifikationskod skickad till {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Kunde inte skicka verifikationskod till {Email}", email);
            throw;
        }
    }

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
                _logger.LogWarning("⚠️ Verifiering misslyckades ({StatusCode}): {Body}", response.StatusCode, json);
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
            _logger.LogError(ex, "❌ Kunde inte verifiera kod för {Email}", email);
            return false;
        }
    }

    private class VerificationResponse
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
