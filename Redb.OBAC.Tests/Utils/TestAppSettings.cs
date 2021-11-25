using Newtonsoft.Json;
using Redb.OBAC.PgSql;

namespace Redb.OBAC.Tests.Utils
{
    public class TestAppSettings
    {
        [JsonProperty("postgres")]
        public PgSqlObacConfig Postgres { get; set; }
        
        [JsonProperty("connection.test")]
        public string ConnectionTest { get; set; }
    }
}