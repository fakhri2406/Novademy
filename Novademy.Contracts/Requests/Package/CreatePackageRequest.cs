using System.Data.SqlTypes;

namespace Novademy.Contracts.Requests.Package;

public class CreatePackageRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public required IEnumerable<Guid> CourseIds { get; init; }
}