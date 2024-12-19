using System;

namespace Redb.OBAC.Core.Ep
{
    public enum PermissionActionEnum
    {
        AddDirectPermission, // for given Obj Type:Obj Id and USER id
        RemoveDirectPermission, // for given Obj Type:Obj Id and USER id
        RemoveAllObjectsDirectPermission // for given Obj Type:Obj Id
    }
    public class PermissionActionInfo
    {
        public PermissionActionEnum Action { get; set; }
        public int UserId { get; set; }
        public Guid ObjectId { get; set; }
        public Guid ObjectTypeId { get; set; }
        public Guid PermissionId { get; set; }
    }
}