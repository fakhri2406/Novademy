using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Novademy.Application.ExternalServices.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    
    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            EnableSsl = _options.EnableSSL
        };
    
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        
        mailMessage.To.Add(to);
        
        await client.SendMailAsync(mailMessage);
    }
}