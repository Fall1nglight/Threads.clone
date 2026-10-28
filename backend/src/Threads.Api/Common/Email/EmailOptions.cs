namespace Threads.Api.Common.Email;

public class EmailOptions
{
    public required string ResendApiToken { get; init; }
    public required string From { get; init; }
}
