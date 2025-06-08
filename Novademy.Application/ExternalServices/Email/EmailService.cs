using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace Novademy.Application.ExternalServices.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly EmailClient _emailClient;
    
    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
        _emailClient = new EmailClient(_options.ConnectionString);
    }
    
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var content = isHtml
            ? new EmailContent(subject) { Html = body }
            : new EmailContent(subject) { PlainText = body };
        
        var message = new EmailMessage(
            senderAddress: _options.FromAddress,
            recipientAddress: to,
            content: content
        );
        
        await _emailClient.SendAsync(
            WaitUntil.Completed,
            message,
            CancellationToken.None
        );
    }
}