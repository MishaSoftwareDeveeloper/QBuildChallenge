using QBuild.DataAccess.Data;
using QBuild.DataAccess.Interfaces;
using QBuild.Models;

namespace QBuild.DataAccess.Repositories;

public sealed class BomRepository : IBomRepository
{
    private readonly BomDataStore _store;

    public BomRepository(BomDataStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IReadOnlyList<BomRecord> GetAll() => _store.BomRecords;

    public IReadOnlyList<BomRecord> GetRootRecords() => _store.GetRootRecords();

    public IReadOnlyList<BomRecord> GetDirectChildren(string parentName) => _store.GetDirectChildren(parentName);

    public IReadOnlyList<BomTreeNode> GetTree() => _store.Tree;

    public bool KnowsComponentName(string name) => _store.KnowsComponentName(name);
}
