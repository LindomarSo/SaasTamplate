namespace SaasStarter.Application.Common.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}
