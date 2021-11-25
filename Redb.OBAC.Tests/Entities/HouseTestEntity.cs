using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redb.OBAC.Tests.ObacClientTests.Entities
{
    [Table("test_houses")]
    public class HouseTestEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}