using System.Collections.ObjectModel;
using QBuild.Models;

namespace QBuild.DataAccess.Data;

public sealed class BomDataStore
{
    private IReadOnlyList<BomTreeNode> _tree = Array.Empty<BomTreeNode>();
    private bool _treeAttached;

    private readonly HashSet<string> _knownNames;

    private BomDataStore(
        IReadOnlyList<BomRecord> bomRecords,
        IReadOnlyDictionary<string, IReadOnlyList<BomRecord>> bomByParent,
        IReadOnlyDictionary<string, Part> partsByName,
        HashSet<string> knownNames)
    {
        BomRecords = bomRecords;
        BomByParent = bomByParent;
        PartsByName = partsByName;
        _knownNames = knownNames;
    }

    public IReadOnlyList<BomRecord> BomRecords { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<BomRecord>> BomByParent { get; }

    public IReadOnlyDictionary<string, Part> PartsByName { get; }

    public IReadOnlyList<BomTreeNode> Tree
    {
        get
        {
            if (!_treeAttached)
            {
                throw new InvalidOperationException("BOM tree has not been attached to the data store.");
            }

            return _tree;
        }
    }

    public BomValidationReport? ValidationReport { get; private set; }

    public static BomDataStore FromRecords(
        IReadOnlyList<BomRecord> bomRecords,
        IReadOnlyList<Part> parts)
    {
        ArgumentNullException.ThrowIfNull(bomRecords);
        ArgumentNullException.ThrowIfNull(parts);

        var bomCopy = new ReadOnlyCollection<BomRecord>(bomRecords.ToList());
        var bomByParent = BuildBomIndex(bomCopy);
        var partsByName = BuildPartIndex(parts);
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in bomCopy)
        {
            if (!string.IsNullOrWhiteSpace(record.ParentName))
            {
                knownNames.Add(record.ParentName);
            }

            knownNames.Add(record.ComponentName);
        }

        return new BomDataStore(bomCopy, bomByParent, partsByName, knownNames);
    }

    public IReadOnlyList<BomRecord> GetDirectChildren(string parentName)
    {
        if (string.IsNullOrWhiteSpace(parentName))
        {
            return GetRootRecords();
        }

        return BomByParent.TryGetValue(parentName.Trim(), out var children)
            ? children
            : Array.Empty<BomRecord>();
    }

    public IReadOnlyList<BomRecord> GetRootRecords()
    {
        return BomByParent.TryGetValue(string.Empty, out var roots)
            ? roots
            : Array.Empty<BomRecord>();
    }

    public Part? FindPart(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return PartsByName.TryGetValue(name.Trim(), out var part) ? part : null;
    }

    public bool KnowsComponentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _knownNames.Contains(name.Trim());
    }

    internal void AttachTree(IReadOnlyList<BomTreeNode> tree, BomValidationReport report)
    {
        if (_treeAttached)
        {
            throw new DataInitializationException("BOM tree has already been attached to the data store.");
        }

        _tree = new ReadOnlyCollection<BomTreeNode>(tree.ToList());
        ValidationReport = report;
        _treeAttached = true;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<BomRecord>> BuildBomIndex(
        IReadOnlyList<BomRecord> bomRecords)
    {
        var grouped = new Dictionary<string, List<BomRecord>>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in bomRecords)
        {
            var key = record.ParentName ?? string.Empty;
            if (!grouped.TryGetValue(key, out var list))
            {
                list = [];
                grouped[key] = list;
            }

            list.Add(record);
        }

        var readOnly = grouped.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<BomRecord>)new ReadOnlyCollection<BomRecord>(pair.Value),
            StringComparer.OrdinalIgnoreCase);

        return new ReadOnlyDictionary<string, IReadOnlyList<BomRecord>>(readOnly);
    }

    private static IReadOnlyDictionary<string, Part> BuildPartIndex(IReadOnlyList<Part> parts)
    {
        var map = new Dictionary<string, Part>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts)
        {
            map[part.Name] = part;
        }

        return new ReadOnlyDictionary<string, Part>(map);
    }
}
