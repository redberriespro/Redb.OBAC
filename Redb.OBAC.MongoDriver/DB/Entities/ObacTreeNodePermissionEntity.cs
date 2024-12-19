using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    // [Table("obac_tree_node_permissions")]
    public class ObacTreeNodePermissionEntity
    {
        [BsonId, BsonElement("id")] 
        public Guid Id { get; set; }
        
        [BsonElement("user_id")]
        public int? UserId { get; set; }
        public ObacUserSubjectEntity User { get; set; }
        public ObacGroupSubjectEntity UserGroup { get; set; }
        
        public ObacTreeNodeEntity Node { get; set; }

        [BsonElement("user_group_id")]
        public int? UserGroupId { get; set; }
        
        [BsonElement("tree_id")] 
        public Guid TreeId { get; set; }
        
        [BsonElement("tree_node_id")]
        public Guid NodeId { get; set; }
        
        [BsonElement("perm_id")]
        public Guid PermissionId { get; set; }

        [BsonElement("is_deny")]
        public bool Deny { get; set; }
    }
}