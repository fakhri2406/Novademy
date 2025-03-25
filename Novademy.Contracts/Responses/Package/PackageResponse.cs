namespace Novademy.Contracts.Responses.Package;

public class PackageResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public IEnumerable<Guid> CourseIds { get; init; } = new List<Guid>();
}