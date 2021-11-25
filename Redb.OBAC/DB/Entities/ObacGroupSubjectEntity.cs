using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.DB.Entities
{
    [Table("obac_user_groups")]

    public class ObacGroupSubjectEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("external_id_int")]
        public int? ExternalIdInt { get; set; }

        [Column("external_id_str")]
        public string ExternalIdString { get; set; }
        
        [Required]
        [Column("description")]
        public string Description { get; set; }
        
        public virtual ICollection<ObacUserInGroupEntity> Users { get; set; } = new HashSet<ObacUserInGroupEntity>();
    }
}