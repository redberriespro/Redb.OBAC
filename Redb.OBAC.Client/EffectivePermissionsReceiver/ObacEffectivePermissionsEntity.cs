using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.Client.EffectivePermissionsReceiver
{
    [Table("obac_ep")]
    public class ObacEffectivePermissionsEntity
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