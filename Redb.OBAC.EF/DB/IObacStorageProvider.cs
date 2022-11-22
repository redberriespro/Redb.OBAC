using Redb.OBAC.Permissions;

namespace Redb.OBAC.EF.DB
{
    public interface IObacStorageProvider
    {
        /// <summary>
        /// factory method
        /// </summary>
        /// <returns></returns>
        ObacDbContext CreateObacContext();
        
        /// <summary>
        /// default e.p. storage - usually in the same database as main data is stored
        /// </summary>
        IEffectivePermissionStorage CreateDbDefaultEffectivePermissionsStorage();
    }
}