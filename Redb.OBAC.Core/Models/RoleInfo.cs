using System;

namespace Redb.OBAC.Core.Models
{
    public class RoleInfo
    {
        public Guid RoleId { get; set; }
        public string Description { get; set; }
        public Guid[] PermissionIds { get; set; }
    }
}