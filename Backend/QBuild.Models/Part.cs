namespace QBuild.Models;

public sealed class Part
{
    public required string Name { get; init; }

    public string? Type { get; init; }

    public string? Item { get; init; }

    public string? PartNumber { get; init; }

    public string? Title { get; init; }

    public string? Material { get; init; }
}
