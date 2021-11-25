using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelloObac
{
    [Table("documents")]
    public class DocumentEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
}