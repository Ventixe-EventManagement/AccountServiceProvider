using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(IAccountService accountService) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _accountService.RegisterAsync(request);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return StatusCode(result.StatusCode);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
    {
        var result = await _accountService.ConfirmEmailAsync(request.UserId, request.Token);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(new { message = "Email confirmed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var result = await _accountService.ForgotPasswordAsync(request.Email);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(new { message = "Password reset email sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await _accountService.ResetPasswordAsync(
            request.UserId,
            request.Token,
            request.NewPassword
            );

        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(new { message = "Password reset successful" });
    }

    [HttpPost("validate-login")]
    public async Task<IActionResult> ValidateLogin(LoginRequest request)
    {
        var result = await _accountService.ValidateLoginAsync(request);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(result.Result);
    }
}
