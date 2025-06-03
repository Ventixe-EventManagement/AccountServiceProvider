namespace Business.Models;

public class VerifyCodeRequest
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
