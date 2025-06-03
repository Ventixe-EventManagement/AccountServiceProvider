using Business.DTOs;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Business.Services;

public class AccountService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IVerificationClient verificationClient,
    ILogger<AccountService> logger
) : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IVerificationClient _verificationClient = verificationClient;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<AccountResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("📥 Registreringsförsök för {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return AccountResult.CreateFailure("Email is already registered.", 409);

            var user = AccountFactory.FromRegisterRequest(request);
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("❌ Misslyckades skapa användare: {Error}", error);
                return AccountResult.CreateFailure(error, 400);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("✅ Användare skapad: {Email}", user.Email);

            await _verificationClient.SendVerificationCodeAsync(user.Email!);

            _logger.LogInformation("📤 Verifikationskod skickad till {Email}", user.Email);

            return AccountResult.CreateSuccess(201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ RegisterAsync – oväntat fel");
            return AccountResult.CreateFailure($"Unexpected error: {ex.Message}", 500);
        }
    }

    public async Task<AccountResult> VerifyEmailCodeAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return AccountResult.CreateFailure("User not found", 404);

        var verificationResult = await _verificationClient.VerifyCodeAsync(email, code);

        if (!verificationResult)
            return AccountResult.CreateFailure("Invalid or expired verification code", 400);

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        return AccountResult.CreateSuccess();
    }

    public async Task<AccountResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return AccountResult.CreateFailure("User not found", 404);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // TODO: Implementera en separat IPasswordResetClient och skicka länken via mejl
        return AccountResult.CreateSuccess();
    }

    public async Task<AccountResult> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AccountResult.CreateFailure("User not found", 404);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? AccountResult.CreateSuccess()
            : AccountResult.CreateFailure("Invalid or expired token", 400);
    }

    public async Task<AccountResult<ValidatedUserDto>> ValidateLoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return AccountResult<ValidatedUserDto>.CreateFailure("Invalid credentials", 401);

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return AccountResult<ValidatedUserDto>.CreateFailure("Email is not confirmed", 403);

        var roles = await _userManager.GetRolesAsync(user);

        var validatedUser = new ValidatedUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return AccountResult<ValidatedUserDto>.CreateSuccess(validatedUser);
    }

    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }
}