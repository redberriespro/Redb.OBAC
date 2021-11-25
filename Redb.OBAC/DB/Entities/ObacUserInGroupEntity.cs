using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.DB.Entities
{
    [Table("obac_users_in_groups")]

    public class ObacUserInGroupEntity
    {
        [Column("user_id")]
        public int UserId { get; set; }
        
        public ObacUserSubjectEntity User { get; set; }
        
        [Column("group_id")]
        public int GroupId { get; set; }
        public ObacGroupSubjectEntity Group { get; set; }


    }
}