using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redb.OBAC.MySql
{
    public class MySqlObacConfig
    {
        [JsonProperty("connection")]
        public string Connection { get; set; }

        [JsonProperty("noMigrate")]
        public bool NoMigrate { get; set; } = false;
    }
}
