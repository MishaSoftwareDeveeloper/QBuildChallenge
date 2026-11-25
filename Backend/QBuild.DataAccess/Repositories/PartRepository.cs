using QBuild.DataAccess.Data;
using QBuild.DataAccess.Interfaces;
using QBuild.Models;

namespace QBuild.DataAccess.Repositories;

public sealed class PartRepository : IPartRepository
{
    private readonly BomDataStore _store;

    public PartRepository(BomDataStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Part? FindByName(string name) => _store.FindPart(name);
}
