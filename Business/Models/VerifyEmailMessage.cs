namespace Business.Models;

public class VerifyEmailMessage
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}