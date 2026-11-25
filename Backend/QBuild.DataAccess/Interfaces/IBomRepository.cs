using QBuild.Models;

namespace QBuild.DataAccess.Interfaces;

public interface IBomRepository
{
    IReadOnlyList<BomRecord> GetAll();

    IReadOnlyList<BomRecord> GetRootRecords();

    IReadOnlyList<BomRecord> GetDirectChildren(string parentName);

    IReadOnlyList<BomTreeNode> GetTree();

    bool KnowsComponentName(string name);
}
