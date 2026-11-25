namespace QBuild.Models;

public sealed class BomTreeNode
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required int Level { get; init; }

    public string? ParentName { get; init; }

    public IReadOnlyList<BomTreeNode> Children { get; init; } = Array.Empty<BomTreeNode>();
}
