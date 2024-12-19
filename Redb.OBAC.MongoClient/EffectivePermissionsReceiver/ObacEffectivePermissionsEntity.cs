using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDbClient.EffectivePermissionsReceiver
{
    //[Table("obac_ep")]
    public class ObacEffectivePermissionsEntity
    {
        [BsonId]
        public Guid Id { get; set; }
        
        [BsonElement("userid")]
        public int UserId { get; set; }
        
        [BsonElement("permid")]
        public Guid PermissionId { get; set; }

        [BsonElement("objtypeid")]
        public Guid ObjectTypeId { get; set; }
        
        [BsonElement("objid")]
        public Guid? ObjectId { get; set; }
    }
}