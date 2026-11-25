using QBuild.Models;

namespace QBuild.Business.Interfaces;

public interface IBomService
{
    IReadOnlyList<BomTreeNode> GetTree();

    IReadOnlyList<ComponentDto> GetDirectComponents(string parentName);

    bool Exists(string parentName);
}
