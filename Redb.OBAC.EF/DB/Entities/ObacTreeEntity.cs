using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_trees")]
    public class ObacTreeEntity
    {
        [Key] [Column("id")] 
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } 
        
        [Column("description")] 
        public string Description { get; set; } 
        
        [Column("external_id_int")] public int? ExternalIdInt { get; set; }

        [Column("external_id_str")] public string ExternalIdString { get; set; }
    }
}