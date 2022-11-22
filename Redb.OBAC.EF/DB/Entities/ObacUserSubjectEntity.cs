using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_users")]

    public class ObacUserSubjectEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("external_id_int")]
        public int? ExternalIdInt { get; set; }

        [Column("external_id_str")]
        public string ExternalIdString { get; set; }
        
        [Required]
        [Column("description")]
        public string Description { get; set; }
        
        
        public virtual ICollection<ObacUserInGroupEntity> Groups { get; set; } = new HashSet<ObacUserInGroupEntity>();

    }
}