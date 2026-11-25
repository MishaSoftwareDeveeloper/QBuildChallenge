using QBuild.Business.Interfaces;
using QBuild.Business.Mappers;
using QBuild.DataAccess.Interfaces;
using QBuild.Models;

namespace QBuild.Business.Services;

public sealed class BomService : IBomService
{
    private readonly IBomRepository _bomRepository;
    private readonly IPartRepository _partRepository;

    public BomService(IBomRepository bomRepository, IPartRepository partRepository)
    {
        _bomRepository = bomRepository ?? throw new ArgumentNullException(nameof(bomRepository));
        _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
    }

    public IReadOnlyList<BomTreeNode> GetTree() => _bomRepository.GetTree();

    public IReadOnlyList<ComponentDto> GetDirectComponents(string parentName)
    {
        if (string.IsNullOrWhiteSpace(parentName))
        {
            throw new ArgumentException("Parent name is required.", nameof(parentName));
        }

        var children = _bomRepository.GetDirectChildren(parentName.Trim());
        var result = new List<ComponentDto>(children.Count);

        foreach (var record in children)
        {
            var part = _partRepository.FindByName(record.ComponentName);
            result.Add(BomTreeMapper.ToComponentDto(record, part));
        }

        return result;
    }

    public bool Exists(string parentName)
    {
        if (string.IsNullOrWhiteSpace(parentName))
        {
            return false;
        }

        return _bomRepository.KnowsComponentName(parentName.Trim());
    }
}
