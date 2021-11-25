using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.DB.Entities
{
    [Table("obac_permissions")]
    public class ObacPermissionEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }
    }
}