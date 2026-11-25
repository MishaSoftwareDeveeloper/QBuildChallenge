using QBuild.Models;

namespace QBuild.DataAccess.Interfaces;

public interface IPartRepository
{
    Part? FindByName(string name);
}
