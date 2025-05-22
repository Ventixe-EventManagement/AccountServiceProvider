namespace Business.Models;

public class ConfirmEmailRequest
{
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
}