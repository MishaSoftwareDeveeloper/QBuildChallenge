using QBuild.Business.Validators;
using QBuild.DataAccess.Configuration;
using QBuild.DataAccess.Data;
using QBuild.Models;

namespace QBuild.Business.Services;

public static class BomStartupService
{
    public static BomDataStore Initialize(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            throw new DataInitializationException("Data directory is not configured.");
        }

        var settings = CsvSettings.FromDirectory(dataDirectory);
        var initializer = new BomDataInitializer(settings);
        var store = initializer.Initialize();
        var cycleWarnings = new List<string>();
        var tree = BomTreeBuilder.Build(store, cycleWarnings);
        var report = BomValidator.Validate(store, tree, cycleWarnings);
        initializer.AttachTree(tree, report);
        return store;
    }
}
