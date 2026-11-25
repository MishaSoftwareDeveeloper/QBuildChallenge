namespace QBuild.Models;

public sealed class BomRecord
{
    public required string ParentName { get; init; }

    public required int Quantity { get; init; }

    public required string ComponentName { get; init; }

    public bool IsRoot => string.IsNullOrWhiteSpace(ParentName);
}
