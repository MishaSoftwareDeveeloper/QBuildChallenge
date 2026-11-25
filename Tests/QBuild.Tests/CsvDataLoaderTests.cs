using QBuild.DataAccess.Configuration;
using QBuild.DataAccess.Data;
using QBuild.Models;

namespace QBuild.Tests;

public class CsvDataLoaderTests
{
    [Fact]
    public void Load_ReadsBothFiles_WithExpectedCountsAndColumns()
    {
        var loader = new CsvDataLoader(CsvSettings.FromDirectory(TestPaths.DataDirectory));

        var loaded = loader.Load();

        Assert.Equal(196, loaded.BomRecords.Count);
        Assert.Equal(186, loaded.Parts.Count);
        Assert.Contains(loaded.BomRecords, r => r.ComponentName == "BODY" && r.ParentName == "VALVE");
        Assert.Contains(loaded.Parts, p => p.Name == "BODY" && p.PartNumber == "09200-254878");
    }

    [Fact]
    public void Load_TreatsEmptyParentNameAsRoot_AndParsesQuantity()
    {
        var loader = new CsvDataLoader(CsvSettings.FromDirectory(TestPaths.DataDirectory));

        var loaded = loader.Load();
        var roots = loaded.BomRecords.Where(r => r.IsRoot).ToList();

        var root = Assert.Single(roots);
        Assert.Equal("VALVE", root.ComponentName);
        Assert.Equal(1, root.Quantity);
        Assert.Equal(string.Empty, root.ParentName);
        Assert.Contains(loaded.BomRecords, r => r.ParentName == "VALVE" && r.ComponentName == "DRIVESCREW" && r.Quantity == 4);
    }

    [Fact]
    public void Load_ThrowsMeaningfulError_WhenBomFileIsMissing()
    {
        var directory = CreateTempDirectory();
        File.Copy(Path.Combine(TestPaths.DataDirectory, "part.csv"), Path.Combine(directory, "part.csv"));
        var loader = new CsvDataLoader(CsvSettings.FromDirectory(directory));

        var ex = Assert.Throws<DataInitializationException>(() => loader.Load());

        Assert.Contains("bom.csv", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(directory, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_ThrowsMeaningfulError_WhenPartFileIsMissing()
    {
        var directory = CreateTempDirectory();
        File.Copy(Path.Combine(TestPaths.DataDirectory, "bom.csv"), Path.Combine(directory, "bom.csv"));
        var loader = new CsvDataLoader(CsvSettings.FromDirectory(directory));

        var ex = Assert.Throws<DataInitializationException>(() => loader.Load());

        Assert.Contains("part.csv", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_CannotBeCalledTwice()
    {
        var loader = new CsvDataLoader(CsvSettings.FromDirectory(TestPaths.DataDirectory));
        loader.Load();

        var ex = Assert.Throws<DataInitializationException>(() => loader.Load());
        Assert.Contains("already been loaded", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "qbuild-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
