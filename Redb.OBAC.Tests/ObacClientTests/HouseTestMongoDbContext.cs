using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using Redb.OBAC.Tests.ObacClientTests.Entities;
using Redb.OBAC.Tests.Entities;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class HouseTestMongoDbContext:MongoDbClient.ObacEpContextBase
    {

        public IMongoCollection<HouseTestEntityMongo> Houses { get; set; }

        public HouseTestMongoDbContext() { }

        public HouseTestMongoDbContext(string connectionString):base(connectionString)
        {
           
        }
        public void EnsureCreated()
        {
            Database.CreateCollection("test_houses");
            Houses = Database.GetCollection<HouseTestEntityMongo>("test_houses");
        }

    }
    
}
