using Business.DTOs;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Business.Services;

/// <summary>
/// Service for handling user registration, login, email confirmation,
/// and password management via ASP.NET Identity.
/// </summary>
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

    /// <summary>
    /// Registers a new user and sends a verification code via the verification service.
    /// </summary>
    public async Task<AccountResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return AccountResult.CreateFailure("Email is already registered.", 409);

            var user = AccountFactory.FromRegisterRequest(request);
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create user: {Error}", error);
                return AccountResult.CreateFailure(error, 400);
            }

            // Ensure default role exists
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("User successfully created: {Email}", user.Email);

            // Send email verification
            await _verificationClient.SendVerificationCodeAsync(user.Email!);
            _logger.LogInformation("Verification code sent to {Email}", user.Email);

            return AccountResult.CreateSuccess(201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterAsync – unexpected error");
            return AccountResult.CreateFailure($"Unexpected error: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Confirms the user's email by validating the provided verification code.
    /// </summary>
    public async Task<AccountResult> ConfirmEmailAsync(string email, string code)
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

    /// <summary>
    /// Sends a password reset token if the user exists.
    /// </summary>
    public async Task<AccountResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return AccountResult.CreateFailure("User not found", 404);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return AccountResult.CreateSuccess();
    }

    /// <summary>
    /// Resets the password using a valid reset token.
    /// </summary>
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

    /// <summary>
    /// Validates login credentials and returns a validated user DTO with roles.
    /// </summary>
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

    /// <summary>
    /// Retrieves the user ID by their email address.
    /// </summary>
    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }
}
