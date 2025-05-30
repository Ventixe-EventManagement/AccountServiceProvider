using Business.Models;

namespace Business.Interfaces;

public interface IEmailQueuePublisher
{
    Task PublishVerificationEmailAsync(VerifyEmailMessage message);
}
