using QBuild.Business.Services;
using QBuild.DataAccess.Repositories;

namespace QBuild.Tests;

public class BomServiceTests
{
    private static readonly Lazy<BomService> Service = new(() =>
    {
        var store = BomStartupService.Initialize(TestPaths.DataDirectory);
        return new BomService(new BomRepository(store), new PartRepository(store));
    });

    [Theory]
    [InlineData("VALVE")]
    [InlineData("BODY")]
    [InlineData("FORM_BODY")]
    [InlineData("GUIDE_RH")]
    public void GetDirectComponents_ReturnsOnlyDirectChildren_WithBomAndPartFields(string parentName)
    {
        var components = Service.Value.GetDirectComponents(parentName);

        Assert.NotEmpty(components);
        Assert.All(components, item =>
        {
            Assert.Equal(parentName, item.ParentName, ignoreCase: true);
            Assert.False(string.IsNullOrWhiteSpace(item.ComponentName));
            Assert.True(item.Quantity > 0);
        });
    }

    [Fact]
    public void GetDirectComponents_ValveBody_UsesPartEnrichment()
    {
        var body = Service.Value.GetDirectComponents("VALVE")
            .Single(c => c.ComponentName == "BODY");

        Assert.Equal("VALVE", body.ParentName);
        Assert.Equal("BODY", body.ComponentName);
        Assert.Equal(1, body.Quantity);
        Assert.Equal("09200-254878", body.PartNumber);
        Assert.Equal("BODY ASSEMBLY", body.Title);
        Assert.Equal("ASSEMBLY", body.Type);
        Assert.Equal("A", body.Item);
        Assert.Equal("SA-516-GR.70N", body.Material);
    }

    [Fact]
    public void GetDirectComponents_GuideRh_KeepsBomRowsWhenPartIsMissing()
    {
        var components = Service.Value.GetDirectComponents("GUIDE_RH");

        Assert.Equal(3, components.Count);
        Assert.Contains(components, c => c.ComponentName == "GUIDE_RH_PL" && c.Quantity == 1);
        Assert.Contains(components, c => c.ComponentName == "WELD_HF_GUIDE_RH");
        Assert.Contains(components, c => c.ComponentName == "GUIDE_MTO_RH");
        Assert.All(components, c =>
        {
            Assert.Equal("GUIDE_RH", c.ParentName);
            Assert.True(string.IsNullOrEmpty(c.PartNumber));
            Assert.True(string.IsNullOrEmpty(c.Title));
            Assert.True(string.IsNullOrEmpty(c.Type));
            Assert.True(string.IsNullOrEmpty(c.Item));
            Assert.True(string.IsNullOrEmpty(c.Material));
        });
    }

    [Fact]
    public void GetDirectComponents_LeafNode_ReturnsEmptyArray()
    {
        var components = Service.Value.GetDirectComponents("FORM_SPOOL");

        Assert.NotNull(components);
        Assert.Empty(components);
        Assert.True(Service.Value.Exists("FORM_SPOOL"));
    }

    [Fact]
    public void GetDirectComponents_DoesNotReturnGrandchildren()
    {
        var names = Service.Value.GetDirectComponents("BODY").Select(c => c.ComponentName).ToHashSet();

        Assert.Contains("FORM_BODY", names);
        Assert.DoesNotContain("FORM_SPOOL_ASSY", names);
        Assert.DoesNotContain("FORM_SPOOL", names);
    }
}
