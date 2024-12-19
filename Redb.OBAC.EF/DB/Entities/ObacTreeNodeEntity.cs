using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Redb.OBAC.Core.Models;

namespace Redb.OBAC.EF.DB.Entities
{
    [Table("obac_tree_nodes")]
    public class ObacTreeNodeEntity
    {
        [Column("tree_id")] 
        public Guid TreeId { get; set; }
        
        [Column("id")] 
        public Guid Id { get; set; }
        
        public ObacTreeEntity Tree { get; set; }
        
        [Column("parent_id")] 
        public Guid? ParentId { get; set; }
        
        public ObacTreeNodeEntity Parent { get; set; }
        
        [Column("owner_user_id")] 
        public int OwnerUserId { get; set; }

        public ObacUserSubjectEntity Owner { get; set; }

        
        [Column("inherit_parent_perms")]
        public bool InheritParentPermissions { get; set; }
        
        /// <summary>
        /// Access Control List set for the node
        /// </summary>
        [Column("acl")] public string Acl { get; set; }

        [NotMapped]
        public AclInfo AclJSON
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Acl)) return new AclInfo
                {
                    InheritParentPermissions = InheritParentPermissions,
                    AclItems = Array.Empty<AclItemInfo>()
                };
                
                return JsonConvert.DeserializeObject<AclInfo>(Acl);
            }
            set { Acl = JsonConvert.SerializeObject(value); }
        }
    }
}