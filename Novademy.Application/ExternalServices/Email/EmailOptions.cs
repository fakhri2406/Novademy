namespace Novademy.Application.ExternalServices.Email;

public class EmailOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool EnableSSL { get; set; }
} 