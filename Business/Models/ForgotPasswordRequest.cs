using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}