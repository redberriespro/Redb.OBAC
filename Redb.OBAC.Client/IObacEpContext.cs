using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client.EffectivePermissionsReceiver;

namespace Redb.OBAC.Client
{
    public interface IObacEpContext : IEffectivePermissionsAware
    {
        DbSet<ObacEffectivePermissionsEntity> EffectivePermissions { get; set; }
    }
}