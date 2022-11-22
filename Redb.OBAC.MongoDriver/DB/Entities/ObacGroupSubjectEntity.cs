using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_user_groups")]

    public class ObacGroupSubjectEntity
    {
        [BsonId]
        [BsonElement("id")]
        public int Id { get; set; }
        
        [BsonElement("external_id_int")]
        public int? ExternalIdInt { get; set; }

        [BsonElement("external_id_str")]
        public string ExternalIdString { get; set; }
        
       //[Required]
        [BsonElement("description")]
        public string Description { get; set; }
        
        public virtual ICollection<ObacUserInGroupEntity> Users { get; set; } = new HashSet<ObacUserInGroupEntity>();
    }
}