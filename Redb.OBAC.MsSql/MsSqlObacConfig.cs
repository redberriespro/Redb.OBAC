using Newtonsoft.Json;

namespace Redb.OBAC.MsSql
{
    public class MsSqlObacConfig
    {
        [JsonProperty("connection")]
        public string Connection { get; set; }

        [JsonProperty("noMigrate")]
        public bool NoMigrate { get; set; } = false;
    }
}
