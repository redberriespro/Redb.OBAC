using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_objecttypes")]
    public class ObacObjectTypeEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }
        
        [Column("type")]
        public ObjectTypeEnum Type { get; set; }
    }
}