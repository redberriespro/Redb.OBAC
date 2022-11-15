using MongoDB.Bson.Serialization.Attributes;

namespace Redb.OBAC.MongoDriver.DB.Entities
{
    // [Table("obac_users_in_groups")]

    public class ObacUserInGroupEntity
    {
        [BsonElement("user_id")]
        public int UserId { get; set; }
        
        public ObacUserSubjectEntity User { get; set; }
        
        [BsonElement("group_id")]
        public int GroupId { get; set; }
        public ObacGroupSubjectEntity Group { get; set; }


    }
}