using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client.EffectivePermissionsReceiver;

namespace Redb.OBAC.Client
{
    public interface IEffectivePermissionsAware
    {
        DbSet<ObacEffectivePermissionsEntity> EffectivePermissions { get; set; }

        Task SaveChangesAsync();

        Task DropEffectivePermissions(Guid objectTypeId, int objectId);
    }
}