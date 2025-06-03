namespace Business.Interfaces;
public interface IVerificationClient
{
    Task SendVerificationCodeAsync(string email);
    Task<bool> VerifyCodeAsync(string email, string code);
}