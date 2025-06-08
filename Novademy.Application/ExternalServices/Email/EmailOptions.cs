namespace Novademy.Application.ExternalServices.Email;

public class EmailOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}