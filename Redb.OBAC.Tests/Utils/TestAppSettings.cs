using Newtonsoft.Json;
using Redb.OBAC.MongoDb;
using Redb.OBAC.MsSql;
using Redb.OBAC.MySql;
using Redb.OBAC.PgSql;

namespace Redb.OBAC.Tests.Utils
{
    public class TestAppSettings
    {
        [JsonProperty("postgres")]
        public TestPgSqlConfig Postgres { get; set; }

        [JsonProperty("mysql")]
        public TestMySqlConfig Mysql { get; set; }

        [JsonProperty("mssql")]
        public TestMsSqlConfig Mssql { get; set; }

        [JsonProperty("mongodb")]
        public TestMongoDbConfig MongoDb { get; set; }
    }

    public class TestDbConfig
    {
        [JsonProperty("connection.test")]
        public string ConnectionTest { get; set; }
    }

    public class TestMySqlConfig : TestDbConfig
    {
        [JsonProperty("db.config")]
        public MySqlObacConfig Config { get; set; }
    }

    public class TestMsSqlConfig : TestDbConfig
    {
        [JsonProperty("db.config")]
        public MsSqlObacConfig Config { get; set; }
    }

    public class TestPgSqlConfig : TestDbConfig
    {
        [JsonProperty("db.config")]
        public PgSqlObacConfig Config { get; set; }
    }

    public class TestMongoDbConfig : TestDbConfig
    {
        [JsonProperty("db.config")]
        public MongoDbObacConfig Config { get; set; }
    }
}