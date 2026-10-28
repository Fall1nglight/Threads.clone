using Microsoft.Extensions.Options;
using Resend;

namespace Threads.Api.Common.Email;

public class EmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly IOptions<EmailOptions> _options;

    public EmailSender(IResend resend, IOptions<EmailOptions> options)
    {
        _resend = resend;
        _options = options;
    }

    public Task<ResendResponse<Guid>> SendAsync(EmailMessage message)
    {
        message.From = _options.Value.From;
        return _resend.EmailSendAsync(message);
    }

    public Task<ResendResponse<Guid>> SendConfirmEmailAsync(string to, string code)
    {
        EmailMessage message = new()
        {
            To = [to],
            Subject = "Confirm email",
            HtmlBody = HtmlTemplateFactory.CreateConfirmEmail(code),
        };

        return SendAsync(message);
    }

    public Task<ResendResponse<Guid>> SendEmailConfirmationSucceededAsync(string to)
    {
        EmailMessage message = new()
        {
            To = [to],
            Subject = "Email confirmation success",
            HtmlBody = HtmlTemplateFactory.CreateEmailConfirmationSucceededEmail(),
        };

        return SendAsync(message);
    }
}
