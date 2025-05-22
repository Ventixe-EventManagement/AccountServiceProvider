namespace Business.Interfaces;
public interface IEmailService
{
    Task SendConfirmationEmailAsync(string email, string confirmationUrl);
    Task SendResetPasswordEmailAsync(string email, string resetUrl);
}