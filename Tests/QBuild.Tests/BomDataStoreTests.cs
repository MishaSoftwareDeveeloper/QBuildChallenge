using System.Collections;
using QBuild.Business.Services;
using QBuild.DataAccess.Configuration;
using QBuild.DataAccess.Data;
using QBuild.Models;

namespace QBuild.Tests;

public class BomDataStoreTests
{
    [Fact]
    public void Initialize_OccursOnce_AndBuildsCaseInsensitiveIndexes()
    {
        var initializer = new BomDataInitializer(CsvSettings.FromDirectory(TestPaths.DataDirectory));
        var store = initializer.Initialize();

        Assert.Equal(196, store.BomRecords.Count);
        Assert.True(store.BomByParent.ContainsKey("valve"));
        Assert.True(store.BomByParent.ContainsKey("VALVE"));
        Assert.Equal(store.GetDirectChildren("valve").Count, store.GetDirectChildren("VALVE").Count);
        Assert.NotNull(store.FindPart("body"));
        Assert.Equal("09200-254878", store.FindPart("BODY")!.PartNumber);
        Assert.Equal("VALVE", Assert.Single(store.GetRootRecords()).ComponentName);

        var ex = Assert.Throws<DataInitializationException>(() => initializer.Initialize());
        Assert.Contains("already been initialized", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Collections_AreSafeForConcurrentReads_AndRejectMutation()
    {
        var store = BomStartupService.Initialize(TestPaths.DataDirectory);

        Parallel.For(0, 50, _ =>
        {
            Assert.NotEmpty(store.Tree);
            Assert.NotNull(store.FindPart("VALVE"));
            Assert.NotEmpty(store.GetDirectChildren("VALVE"));
        });

        Assert.Throws<NotSupportedException>(() => ((IList)store.BomRecords).Add(null!));
        Assert.Throws<NotSupportedException>(() => ((IDictionary)store.PartsByName).Add("X", null!));
        Assert.Throws<NotSupportedException>(() => ((IList)store.Tree).Add(null!));
    }
}
