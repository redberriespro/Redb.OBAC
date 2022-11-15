using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    // [Table("obac_permissions")]
    public class ObacPermissionEntity
    {
        //[Key]
        [BsonId]
        [BsonElement("id")]
        public Guid Id { get; set; }

        //[Required]
        [BsonElement("description")]
        public string Description { get; set; }
    }
}