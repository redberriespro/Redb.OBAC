using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_users")]

    public class ObacUserSubjectEntity
    {
        [BsonId] 
        public int Id { get; set; }
        
        [BsonElement("external_id_int")]
        public int? ExternalIdInt { get; set; }

        [BsonElement("external_id_str")]
        public string ExternalIdString { get; set; }
        
        //[Required]
        [BsonElement("description")]
        public string Description { get; set; }
        
        
        public virtual ICollection<ObacUserInGroupEntity> Groups { get; set; } = new HashSet<ObacUserInGroupEntity>();

    }
}