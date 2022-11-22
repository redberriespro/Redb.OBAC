using System;
using MongoDB.Bson.Serialization.Attributes;
using Redb.OBAC.Core.Models;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    // [Table("obac_objecttypes")]
    public class ObacObjectTypeEntity
    {
        [BsonId]
        [BsonElement("id")]
        public Guid Id { get; set; }

        //[Required]
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("type")]
        public ObjectTypeEnum Type { get; set; }
    }
}