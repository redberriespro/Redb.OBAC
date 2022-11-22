using Newtonsoft.Json;
using System;

namespace Redb.OBAC.MongoDb
{
    public class MongoDbObacConfig
    {
        [JsonProperty("connection")]
        public string Connection { get; set; }

        [JsonProperty("noMigrate")]
        public bool NoMigrate { get; set; } = false;
    }
}
