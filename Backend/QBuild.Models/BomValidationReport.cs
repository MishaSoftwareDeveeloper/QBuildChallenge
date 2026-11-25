namespace QBuild.Models;

public sealed class BomValidationReport
{
    public required int BomRecordCount { get; init; }

    public required int PartRecordCount { get; init; }

    public required int UniqueComponentCount { get; init; }

    public required int ComponentsWithPart { get; init; }

    public required int ComponentsWithoutPart { get; init; }

    public required int RootCount { get; init; }

    public required string RootName { get; init; }

    public required int MaximumDepth { get; init; }

    public required int TreeNodeCount { get; init; }

    public required int ConnectedRelationshipCount { get; init; }

    public IReadOnlyList<string> DisconnectedParents { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> CycleWarnings { get; init; } = Array.Empty<string>();
}
