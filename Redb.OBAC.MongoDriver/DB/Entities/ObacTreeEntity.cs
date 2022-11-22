using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    //[Table("obac_trees")]
    public class ObacTreeEntity
    {
        //[Key] 
        [BsonId]
        [BsonElement("id")] 
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } 
        
        [BsonElement("description")] 
        public string Description { get; set; } 
        
        [BsonElement("external_id_int")] 
        public int? ExternalIdInt { get; set; }

        [BsonElement("external_id_str")] 
        public string ExternalIdString { get; set; }
    }
}