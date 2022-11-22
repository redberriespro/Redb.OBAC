using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_roles")]
    public class ObacRoleEntity
    {
        public ObacRoleEntity()
        {
            Permissions = new HashSet<ObacPermissionRoleEntity>();
        }
        
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }
        
        public ICollection<ObacPermissionRoleEntity> Permissions { get; set; }
    }
}