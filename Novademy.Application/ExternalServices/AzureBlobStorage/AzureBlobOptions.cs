namespace Novademy.Application.ExternalServices.AzureBlobStorage;

public class AzureBlobOptions
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}