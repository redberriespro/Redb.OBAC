using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_userpermissions")]
    public class ObacUserPermissionsEntity
    {
        [BsonId, BsonElement("id")]
        public Guid Id { get; set; }
        
        [BsonElement("userid")]
        public int UserId { get; set; }
        
        [BsonElement("permid")]
        public Guid PermissionId { get; set; }

        [BsonElement("objtypeid")]
        public Guid ObjectTypeId { get; set; }
        
        [BsonElement("objid")]
        public int? ObjectId { get; set; }
    }
}