using Business.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Business.Services;

public class HttpEmailService(HttpClient httpClient) : IEmailService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task SendConfirmationEmailAsync(string email, string confirmationUrl)
    {
        var payload = new
        {
            To = email,
            Subject = "Confirm your email",
            Body = $"Please confirm your email by clicking this link: {confirmationUrl}"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // TODO
        var response = await _httpClient.PostAsync("https://your-email-service/api/email/send-confirmation", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendResetPasswordEmailAsync(string email, string resetUrl)
    {
        var payload = new
        {
            To = email,
            Subject = "Reset your password",
            Body = $"You can reset your password using this link: {resetUrl}"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // TODO
        var response = await _httpClient.PostAsync("https://your-email-service/api/email/send-reset", content);
        response.EnsureSuccessStatusCode();
    }
}