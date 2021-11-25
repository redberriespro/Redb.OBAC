using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.DB.Entities
{
    [Table("obac_tree_nodes")]
    public class ObacTreeNodeEntity
    {
        [Column("tree_id")] 
        public Guid TreeId { get; set; }
        
        [Column("id")] 
        public int Id { get; set; }
        
        public ObacTreeEntity Tree { get; set; }
        
        [Column("parent_id")] 
        public int? ParentId { get; set; }
        
        public ObacTreeNodeEntity Parent { get; set; }
        
        [Column("owner_user_id")] 
        public int OwnerUserId { get; set; }

        public ObacUserSubjectEntity Owner { get; set; }

        
        [Column("inherit_parent_perms")]
        public bool InheritParentPermissions { get; set; }

        
        [Column("external_id_int")] public int? ExternalIdInt { get; set; }

        [Column("external_id_str")] public string ExternalIdString { get; set; }
    }
}