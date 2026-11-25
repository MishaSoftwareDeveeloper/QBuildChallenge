namespace QBuild.Models;

public sealed class ComponentDto
{
    public required string ParentName { get; init; }

    public required string ComponentName { get; init; }

    public string? PartNumber { get; init; }

    public string? Title { get; init; }

    public required int Quantity { get; init; }

    public string? Type { get; init; }

    public string? Item { get; init; }

    public string? Material { get; init; }
}
