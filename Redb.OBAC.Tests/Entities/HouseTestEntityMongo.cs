using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Redb.OBAC.Tests.Entities
{    
    public class HouseTestEntityMongo
    {
        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
