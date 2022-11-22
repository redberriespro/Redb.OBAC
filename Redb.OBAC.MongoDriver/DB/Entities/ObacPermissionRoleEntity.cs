using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_permissions_in_roles")]
    public class ObacPermissionRoleEntity
    {
        [BsonElement("perm_id")]
        public Guid PermissionId { get; set; }
        
        [BsonElement("role_id")]
        public Guid RoleId { get; set; }
        public ObacRoleEntity Role { get; set; }

    }
}