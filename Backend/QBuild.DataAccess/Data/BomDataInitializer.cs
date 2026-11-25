using QBuild.DataAccess.Configuration;
using QBuild.Models;

namespace QBuild.DataAccess.Data;

public sealed class BomDataInitializer
{
    private readonly CsvDataLoader _loader;
    private readonly object _gate = new();
    private BomDataStore? _store;

    public BomDataInitializer(CsvDataLoader loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public BomDataInitializer(CsvSettings settings)
        : this(new CsvDataLoader(settings))
    {
    }

    public BomDataStore Initialize()
    {
        lock (_gate)
        {
            if (_store is not null)
            {
                throw new DataInitializationException("In-memory data store has already been initialized.");
            }

            var loaded = _loader.Load();
            _store = BomDataStore.FromRecords(loaded.BomRecords, loaded.Parts);
            return _store;
        }
    }

    public void AttachTree(IReadOnlyList<BomTreeNode> tree, BomValidationReport report)
    {
        lock (_gate)
        {
            if (_store is null)
            {
                throw new DataInitializationException("Data store must be initialized before attaching the BOM tree.");
            }

            _store.AttachTree(tree, report);
        }
    }
}
