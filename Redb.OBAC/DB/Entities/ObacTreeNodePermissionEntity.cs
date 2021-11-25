using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.DB.Entities
{
    [Table("obac_tree_node_permissions")]
    public class ObacTreeNodePermissionEntity
    {
        [Key, Column("id")] 
        public Guid Id { get; set; }
        
        [Column("user_id")]
        public int? UserId { get; set; }
        public ObacUserSubjectEntity User { get; set; }
        public ObacGroupSubjectEntity UserGroup { get; set; }
        
        public ObacTreeNodeEntity Node { get; set; }

        [Column("user_group_id")]
        public int? UserGroupId { get; set; }
        
        [Column("tree_id")] 
        public Guid TreeId { get; set; }
        
        [Column("tree_node_id")]
        public int NodeId { get; set; }
        
        [Column("perm_id")]
        public Guid PermissionId { get; set; }

        [Column("is_deny")]
        public bool Deny { get; set; }
    }
}