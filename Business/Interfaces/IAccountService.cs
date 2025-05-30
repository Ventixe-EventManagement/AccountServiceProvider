using Business.DTOs;
using Business.Models;

namespace Business.Interfaces;

public interface IAccountService
{
    Task<AccountResult> RegisterAsync(RegisterRequest request);

    Task<AccountResult> ConfirmEmailAsync(string userId, string token);

    Task<AccountResult> ForgotPasswordAsync(string email);

    Task<AccountResult> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<AccountResult<ValidatedUserDto>> ValidateLoginAsync(LoginRequest request);

}