using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class ResetPasswordRequest
{
    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;
}