using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    // [Table("obac_tree_nodes")]
    [BsonIgnoreExtraElements]
    public class ObacTreeNodeEntity
    {
        [BsonElement("tree_id")] 
        public Guid TreeId { get; set; }
        
        [BsonElement("node_id")] 
        public int NodeId { get; set; }
        
        public ObacTreeEntity Tree { get; set; }
        
        [BsonElement("parent_id")] 
        public int? ParentId { get; set; }
        
        public ObacTreeNodeEntity Parent { get; set; }
        
        [BsonElement("owner_user_id")] 
        public int OwnerUserId { get; set; }

        public ObacUserSubjectEntity Owner { get; set; }

        
        [BsonElement("inherit_parent_perms")]
        public bool InheritParentPermissions { get; set; }

        
        [BsonElement("external_id_int")] 
        public int? ExternalIdInt { get; set; }

        [BsonElement("external_id_str")] 
        public string ExternalIdString { get; set; }
    }
}