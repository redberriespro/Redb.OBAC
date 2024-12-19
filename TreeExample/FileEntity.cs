using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeExample
{
    internal class FileEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("key")]
        public int Id { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("guid")]
        public Guid Guid { get; set; }
    }
}