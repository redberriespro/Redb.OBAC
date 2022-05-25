using Newtonsoft.Json;
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

    public class TestPgSqlConfig : TestDbConfig
    {
        [JsonProperty("db.config")]
        public PgSqlObacConfig Config { get; set; }
    }
}