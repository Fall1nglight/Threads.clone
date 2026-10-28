namespace Threads.Api.Common.Email;

public static class HtmlTemplateFactory
{
    public static string CreateConfirmEmail(string code)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Verify your email address</title>
            </head>
            <body style="margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif; color: #1f1f1f;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color: #f5f5f5;">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 520px; background-color: #ffffff; border: 1px solid #e5e5e5; border-radius: 12px;">
                                <tr>
                                    <td style="padding: 40px 32px; text-align: center;">
                                        <h1 style="margin: 0 0 16px; font-size: 24px; line-height: 32px;">
                                            Verify your email address
                                        </h1>

                                        <p style="margin: 0 0 24px; color: #666666; font-size: 16px; line-height: 24px;">
                                            Enter the verification code below to verify your email address:
                                        </p>

                                        <div style="margin: 0 auto 24px; padding: 16px 24px; background-color: #f0f0f0; border-radius: 8px; font-size: 8px; font-weight: bold; text-align: center;">
                                            {code}
                                        </div>

                                        <p style="margin: 0; color: #888888; font-size: 14px; line-height: 20px;">
                                            If you did not request this verification, you can safely ignore this email.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    public static string CreateEmailConfirmationSucceededEmail()
    {
        return """
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Email address verified</title>
            </head>
            <body style="margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif; color: #1f1f1f;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color: #f5f5f5;">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 520px; background-color: #ffffff; border: 1px solid #e5e5e5; border-radius: 12px;">
                                <tr>
                                    <td style="padding: 40px 32px; text-align: center;">
                                        <h1 style="margin: 0 0 16px; font-size: 24px; line-height: 32px;">
                                            Email address verified
                                        </h1>

                                        <p style="margin: 0 0 24px; color: #666666; font-size: 16px; line-height: 24px;">
                                            Your email address has been successfully verified. You can now use all features of your account.
                                        </p>

                                        <p style="margin: 0; color: #888888; font-size: 14px; line-height: 20px;">
                                            If you did not verify this email address, please secure your account immediately.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
