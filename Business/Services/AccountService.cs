using Business.DTOs;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AccountService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmailService emailService
) : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IEmailService _emailService = emailService;

    public async Task<AccountResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return AccountResult.CreateFailure("Email is already registered.", 409);

            var user = AccountFactory.FromRegisterRequest(request);
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                return AccountResult.CreateFailure(error, 400);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

           // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var confirmationUrl = $"https://yourfrontend.com/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

           // await _emailService.SendConfirmationEmailAsync(user.Email!, confirmationUrl);

            return AccountResult.CreateSuccess(201);
        }
        catch (Exception ex)
        {
            return AccountResult.CreateFailure($"Unexpected error: {ex.Message}", 500);
        }
    }

    public async Task<AccountResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return AccountResult.CreateFailure("User not found", 404);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? AccountResult.CreateSuccess()
            : AccountResult.CreateFailure("Invalid or expired token", 400);
    }

    public async Task<AccountResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
      // if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
           // return AccountResult.CreateFailure("User not found or email not confirmed", 404);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = $"https://yourfrontend.com/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendResetPasswordEmailAsync(user.Email!, resetUrl);

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

       // if (!await _userManager.IsEmailConfirmedAsync(user))
          //  return AccountResult<ValidatedUserDto>.CreateFailure("Email is not confirmed", 403);

        var roles = await _userManager.GetRolesAsync(user);

        var validatedUser = new ValidatedUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return AccountResult<ValidatedUserDto>.CreateSuccess(validatedUser);
    }
}

