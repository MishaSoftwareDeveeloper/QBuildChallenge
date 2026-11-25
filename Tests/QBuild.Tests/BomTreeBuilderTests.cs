using QBuild.Business.Services;
using QBuild.Business.Validators;
using QBuild.DataAccess.Data;
using QBuild.DataAccess.Repositories;
using QBuild.Models;

namespace QBuild.Tests;

public class BomTreeBuilderTests
{
    private static readonly Lazy<BomDataStore> Store = new(() => BomStartupService.Initialize(TestPaths.DataDirectory));

    [Fact]
    public void Tree_HasSingleValveRoot_AndFiveLevelVerifiedPath()
    {
        var tree = Store.Value.Tree;
        var root = Assert.Single(tree);

        Assert.Equal("VALVE", root.Name);
        Assert.Equal("VALVE", root.Id);
        Assert.Equal(1, root.Level);
        Assert.Null(root.ParentName);
        Assert.True(BomValidator.GetMaximumDepth(tree) >= 5);

        var spool = BomValidator.FindPath(tree, "VALVE", "BODY", "FORM_BODY", "FORM_SPOOL_ASSY", "FORM_SPOOL");
        Assert.NotNull(spool);
        Assert.Equal(5, spool!.Level);
        Assert.Equal("VALVE/BODY/FORM_BODY/FORM_SPOOL_ASSY/FORM_SPOOL", spool.Id);
        Assert.Empty(spool.Children);

        Assert.NotNull(BomValidator.FindPath(tree, "VALVE", "GUIDE_SET", "GUIDE_RH"));
    }

    [Fact]
    public void Tree_PreservesValidRelationships_AndLeafNodes()
    {
        var store = Store.Value;
        var report = store.ValidationReport!;

        Assert.Equal(196, report.BomRecordCount);
        Assert.Equal(186, report.PartRecordCount);
        Assert.Equal(193, report.UniqueComponentCount);
        Assert.Equal(129, report.ComponentsWithPart);
        Assert.Equal(64, report.ComponentsWithoutPart);
        Assert.Equal(1, report.RootCount);

        var guideRh = BomValidator.FindPath(store.Tree, "VALVE", "GUIDE_SET", "GUIDE_RH");
        Assert.NotNull(guideRh);
        Assert.Equal(3, guideRh!.Children.Count);
        Assert.Contains(guideRh.Children, c => c.Name == "GUIDE_RH_PL");
    }

    [Fact]
    public void Tree_UsesUniqueIds_WhenTheSameNameAppearsUnderDifferentParents()
    {
        var store = Store.Value;
        var matches = new List<BomTreeNode>();
        CollectByName(store.Tree, "LIPSEAL", matches);

        Assert.True(matches.Count >= 2);
        Assert.Equal(matches.Count, matches.Select(m => m.Id).Distinct().Count());
        Assert.Contains(matches, m => m.ParentName != matches[0].ParentName);
    }

    [Fact]
    public void TreeBuilder_StopsCircularReferences()
    {
        var bom = new List<BomRecord>
        {
            new() { ParentName = "", Quantity = 1, ComponentName = "VALVE" },
            new() { ParentName = "VALVE", Quantity = 1, ComponentName = "A" },
            new() { ParentName = "A", Quantity = 1, ComponentName = "B" },
            new() { ParentName = "B", Quantity = 1, ComponentName = "A" }
        };

        var store = BomDataStore.FromRecords(bom, []);
        var warnings = new List<string>();
        var tree = BomTreeBuilder.Build(store, warnings);

        var a = Assert.Single(tree[0].Children);
        var b = Assert.Single(a.Children);
        Assert.Empty(b.Children);
        Assert.Contains(warnings, w => w.Contains("Circular reference", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetTree_ReusesPrebuiltInstance()
    {
        var repository = new BomRepository(Store.Value);

        Assert.Same(repository.GetTree(), repository.GetTree());
    }

    private static void CollectByName(IReadOnlyList<BomTreeNode> nodes, string name, List<BomTreeNode> matches)
    {
        foreach (var node in nodes)
        {
            if (string.Equals(node.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(node);
            }

            CollectByName(node.Children, name, matches);
        }
    }
}
