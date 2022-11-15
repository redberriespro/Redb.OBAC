using Redb.OBAC.Core.Models;
using Redb.OBAC.MongoDriver.DB.Entities;

namespace Redb.OBAC.MongoDriver.ModelsPrivate
{
    public class ObacObjectMapper
    {
        public static TreeNodePermissionInfo EntityToEffectivePermissionInfo(ObacUserPermissionsEntity epe) =>
            new TreeNodePermissionInfo
            {
                NodeId = epe.ObjectId.Value,
                DenyPermission = false,
                PermissionId = epe.PermissionId,
                UserId = epe.UserId,
                UserGroupId = null
            };
    }
}