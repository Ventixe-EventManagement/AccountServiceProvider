using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public async Task<IActionResult> ConfirmEmail([FromBody] VerifyCodeRequest request)
    {
        var result = await _accountService.ConfirmEmailAsync(request.Email, request.Code);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(new { message = "Email confirmed. Please complete your profile." });
    }

    [HttpPost("validate-login")]
    public async Task<IActionResult> ValidateLogin(LoginRequest request)
    {
        var result = await _accountService.ValidateLoginAsync(request);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { result.Error });

        return Ok(result.Result);
    }

    [AllowAnonymous]
    [HttpGet("user-id")]
    public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
    {
        var userId = await _accountService.GetUserIdByEmailAsync(email);
        if (userId == null)
            return NotFound("User not found");

        return Ok(userId);
    }
}
