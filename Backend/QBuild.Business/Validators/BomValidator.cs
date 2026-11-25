using QBuild.DataAccess.Data;
using QBuild.Models;

namespace QBuild.Business.Validators;

public static class BomValidator
{
    public const string ExpectedRootName = "VALVE";
    public const int ExpectedMinimumDepth = 5;

    public static BomValidationReport Validate(
        BomDataStore store,
        IReadOnlyList<BomTreeNode> tree,
        IReadOnlyList<string> cycleWarnings)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(tree);
        cycleWarnings ??= Array.Empty<string>();

        var roots = store.GetRootRecords();
        if (roots.Count != 1)
        {
            throw new DataInitializationException(
                $"Expected exactly one BOM root record with an empty PARENT_NAME, found {roots.Count}.");
        }

        var rootName = roots[0].ComponentName;
        if (!string.Equals(rootName, ExpectedRootName, StringComparison.OrdinalIgnoreCase))
        {
            throw new DataInitializationException(
                $"Expected root component '{ExpectedRootName}', found '{rootName}'.");
        }

        if (tree.Count != 1 || !string.Equals(tree[0].Name, ExpectedRootName, StringComparison.OrdinalIgnoreCase))
        {
            throw new DataInitializationException("Prebuilt BOM tree must contain a single VALVE root node.");
        }

        var maxDepth = GetMaximumDepth(tree);
        if (maxDepth < ExpectedMinimumDepth)
        {
            throw new DataInitializationException(
                $"Expected at least {ExpectedMinimumDepth} hierarchy levels, found {maxDepth}.");
        }

        var nodesByName = IndexNodesByName(tree);
        var uniqueComponents = store.BomRecords
            .Select(r => r.ComponentName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var withPart = uniqueComponents.Count(name => store.FindPart(name) is not null);
        var disconnectedParents = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var connectedRelationships = 0;

        foreach (var record in store.BomRecords)
        {
            if (record.IsRoot)
            {
                continue;
            }

            if (!nodesByName.TryGetValue(record.ParentName, out var parentOccurrences))
            {
                disconnectedParents.Add(record.ParentName);
                continue;
            }

            var represented = parentOccurrences.Any(parent =>
                parent.Children.Any(child =>
                    string.Equals(child.Name, record.ComponentName, StringComparison.OrdinalIgnoreCase)));

            if (!represented)
            {
                throw new DataInitializationException(
                    $"BOM relationship '{record.ParentName}' -> '{record.ComponentName}' was not represented in the tree.");
            }

            connectedRelationships++;
        }

        return new BomValidationReport
        {
            BomRecordCount = store.BomRecords.Count,
            PartRecordCount = store.PartsByName.Count,
            UniqueComponentCount = uniqueComponents.Count,
            ComponentsWithPart = withPart,
            ComponentsWithoutPart = uniqueComponents.Count - withPart,
            RootCount = roots.Count,
            RootName = rootName,
            MaximumDepth = maxDepth,
            TreeNodeCount = CountNodes(tree),
            ConnectedRelationshipCount = connectedRelationships,
            DisconnectedParents = disconnectedParents.ToList(),
            CycleWarnings = cycleWarnings
        };
    }

    public static int GetMaximumDepth(IReadOnlyList<BomTreeNode> nodes)
    {
        var max = 0;
        foreach (var node in nodes)
        {
            max = Math.Max(max, GetMaximumDepth(node));
        }

        return max;
    }

    public static BomTreeNode? FindPath(IReadOnlyList<BomTreeNode> nodes, params string[] names)
    {
        IReadOnlyList<BomTreeNode> current = nodes;
        BomTreeNode? match = null;

        foreach (var name in names)
        {
            match = current.FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return null;
            }

            current = match.Children;
        }

        return match;
    }

    private static int GetMaximumDepth(BomTreeNode node)
    {
        if (node.Children.Count == 0)
        {
            return node.Level;
        }

        return node.Children.Max(GetMaximumDepth);
    }

    private static Dictionary<string, List<BomTreeNode>> IndexNodesByName(IReadOnlyList<BomTreeNode> tree)
    {
        var index = new Dictionary<string, List<BomTreeNode>>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in tree)
        {
            Add(node, index);
        }

        return index;

        static void Add(BomTreeNode node, Dictionary<string, List<BomTreeNode>> index)
        {
            if (!index.TryGetValue(node.Name, out var list))
            {
                list = [];
                index[node.Name] = list;
            }

            list.Add(node);
            foreach (var child in node.Children)
            {
                Add(child, index);
            }
        }
    }

    private static int CountNodes(IReadOnlyList<BomTreeNode> nodes)
    {
        return nodes.Sum(CountNode);

        static int CountNode(BomTreeNode node) => 1 + node.Children.Sum(CountNode);
    }
}
