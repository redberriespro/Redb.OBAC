using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_roles")]
    public class ObacRoleEntity
    {
        public ObacRoleEntity()
        {
            Permissions = new HashSet<ObacPermissionRoleEntity>();
        }

        //[Key]
        [BsonId]
        [BsonElement("id")]
        public Guid Id { get; set; }

        //[Required]
        [BsonElement("description")]
        public string Description { get; set; }
        
        public ICollection<ObacPermissionRoleEntity> Permissions { get; set; }
    }
}