using System;
using System.Threading.Tasks;

namespace Redb.OBAC
{
    public interface IEffectivePermissionsAware
    {
        Task SaveChangesAsync();

        Task DropEffectivePermissions(Guid objectTypeId, int objectId);
    }
}