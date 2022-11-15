using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_permissions_in_roles")]
    public class ObacPermissionRoleEntity
    {
        [Column("perm_id")]
        public Guid PermissionId { get; set; }
        
        [Column("role_id")]
        public Guid RoleId { get; set; }
        public ObacRoleEntity Role { get; set; }
        
    }
}