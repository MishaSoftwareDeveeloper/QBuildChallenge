using QBuild.DataAccess.Data;
using QBuild.Models;

namespace QBuild.Business.Services;

public static class BomTreeBuilder
{
    public static IReadOnlyList<BomTreeNode> Build(BomDataStore store, ICollection<string>? cycleWarnings = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        var roots = store.GetRootRecords();
        var nodes = new List<BomTreeNode>(roots.Count);

        foreach (var root in roots)
        {
            var ancestry = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                root.ComponentName
            };

            nodes.Add(BuildNode(
                store,
                parentName: null,
                componentName: root.ComponentName,
                path: root.ComponentName,
                level: 1,
                ancestry,
                cycleWarnings));
        }

        return nodes;
    }

    public static BomTreeNode BuildNode(
        BomDataStore store,
        string? parentName,
        string componentName,
        string path,
        int level,
        HashSet<string> ancestry,
        ICollection<string>? cycleWarnings)
    {
        var childRecords = store.GetDirectChildren(componentName);
        var children = new List<BomTreeNode>(childRecords.Count);
        var siblingCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var child in childRecords)
        {
            var occurrence = siblingCounts.GetValueOrDefault(child.ComponentName);
            siblingCounts[child.ComponentName] = occurrence + 1;

            var childSegment = occurrence == 0
                ? child.ComponentName
                : $"{child.ComponentName}#{occurrence + 1}";
            var childPath = $"{path}/{childSegment}";

            if (ancestry.Contains(child.ComponentName))
            {
                cycleWarnings?.Add(
                    $"Circular reference skipped: {path} -> {child.ComponentName}");
                continue;
            }

            var childAncestry = new HashSet<string>(ancestry, StringComparer.OrdinalIgnoreCase)
            {
                child.ComponentName
            };

            children.Add(BuildNode(
                store,
                parentName: componentName,
                componentName: child.ComponentName,
                path: childPath,
                level: level + 1,
                ancestry: childAncestry,
                cycleWarnings));
        }

        return new BomTreeNode
        {
            Id = path,
            Name = componentName,
            Level = level,
            ParentName = parentName,
            Children = children
        };
    }
}
