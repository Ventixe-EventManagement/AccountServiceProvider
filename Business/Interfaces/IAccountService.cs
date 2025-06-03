using Business.DTOs;
using Business.Models;

namespace Business.Interfaces;
public interface IAccountService
{
    Task<AccountResult> RegisterAsync(RegisterRequest request);
    Task<AccountResult> ConfirmEmailAsync(string email, string code);
    Task<AccountResult<ValidatedUserDto>> ValidateLoginAsync(LoginRequest request);
    Task<string?> GetUserIdByEmailAsync(string email);
}