namespace Novademy.Application.ExternalServices.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml);
} 