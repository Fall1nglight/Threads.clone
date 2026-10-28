using Resend;

namespace Threads.Api.Common.Email;

public interface IEmailSender
{
    Task<ResendResponse<Guid>> SendAsync(EmailMessage message);
    Task<ResendResponse<Guid>> SendConfirmEmailAsync(string to, string code);
    Task<ResendResponse<Guid>> SendEmailConfirmationSucceededAsync(string to);
}
