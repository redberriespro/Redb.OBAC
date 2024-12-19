using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.EF.DB.Entities
{
    /// <summary>
    /// effective user permissions
    /// </summary>
    [Table("obac_userpermissions")]
    public class ObacUserPermissionsEntity
    {
        [Key, Column("id")]
        public Guid Id { get; set; }
        
        [Column("userid")]
        public int UserId { get; set; }
        
        [Column("permid")]
        public Guid PermissionId { get; set; }

        [Column("objtypeid")]
        public Guid ObjectTypeId { get; set; }
        
        [Column("objid")]
        public Guid? ObjectId { get; set; }
    }
}