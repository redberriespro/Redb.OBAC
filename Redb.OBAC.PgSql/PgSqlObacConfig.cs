using Newtonsoft.Json;

namespace Redb.OBAC.PgSql
{
    public class PgSqlObacConfig
    {
        [JsonProperty("connection")]
        public string Connection { get; set; }

        [JsonProperty("noMigrate")] 
        public bool NoMigrate { get; set; } = false;
    }
}