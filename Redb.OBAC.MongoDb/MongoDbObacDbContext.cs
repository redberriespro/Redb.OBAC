using System;
using System.Collections.Generic;
using System.Text;
using Redb.OBAC.MongoDriver;
using Redb.OBAC.MongoDriver.DB;

namespace Redb.OBAC.MongoDb
{
    public class MongoDbObacDbContext : ObacMongoDriverContext
    {
        public MongoDbObacDbContext() : base("mongodb://mongodb://localhost:27017") { }
        public MongoDbObacDbContext(string connectionString) : base(connectionString) { }
        public MongoDbObacDbContext(string connectionString, string dbName) : base(connectionString, dbName) { }
    }
}
