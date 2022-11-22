using Redb.OBAC.Permissions;

namespace Redb.OBAC.MongoDriver.DB
{
    public interface IObacStorageProvider
    {
        /// <summary>
        /// factory method
        /// </summary>
        /// <returns></returns>
        ObacMongoDriverContext CreateObacContext();
        
        /// <summary>
        /// default e.p. storage - usually in the same database as main data is stored
        /// </summary>
        IEffectivePermissionStorage CreateDbDefaultEffectivePermissionsStorage();
    }
}