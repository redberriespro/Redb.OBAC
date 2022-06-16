using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using Redb.OBAC.ApiHost;
using Redb.OBAC.BL;
using Redb.OBAC.Client;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.DB;
using Redb.OBAC.MySql;
using Redb.OBAC.PgSql;
using Redb.OBAC.Tests.ObacClientTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC.Tests.Utils
{
    internal abstract class DbAggregator
    {
        private bool isInit = false;
        protected static TestAppSettings Setting;
        private EffectivePermissionsEfReceiver epHouseReceiver;
        private IObacConfiguration config;
        private ApiHostImpl apiHost;


        public DbAggregator()
        {
            if (Setting == null) Setting = ReadSettings().Result;
        }


        public IObacConfiguration Configuration => config;
        public ApiHostImpl ApiHost => apiHost;

        public abstract string Name { get; }

        public abstract IObacStorageProvider DbProvider { get; }
        public abstract ObjectStorage DbStorage { get; }
        public abstract HouseTestDbContext HouseDbContext { get; }


        protected static async Task<TestAppSettings> ReadSettings()
        {
            var appBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFile = Path.Combine(appBinPath, "appsettings.json");
            var configText = await File.ReadAllTextAsync(configFile);
            return JsonConvert.DeserializeObject<TestAppSettings>(configText);
        }

        public async Task InitTestFramework()
        {
            if (isInit) return;
            isInit = true;

            // drop DBs before running any tests
            TestCoreDbCleaner();
            await EnsureDatabaseExists();

            TestUserDbCleaner();
            await EnsureCreatedAsync();

            epHouseReceiver = new EffectivePermissionsEfReceiver(CreateHouseDbContext()); 

            // register OBAC configuration
            CreateConfigurationObacManager();

            // initialize in-process obac api based on db config
            CreateApiHost();
        }

        internal void CreateConfigurationObacManager()
        {
            if (config == null) config = ObacManager.CreateConfiguration(DbProvider, epHouseReceiver);
        }

        internal void CreateApiHost()
        {
            if (apiHost == null) apiHost = new ApiHostImpl(config);
        }

        internal abstract void TestCoreDbCleaner();
        internal abstract void TestUserDbCleaner();
        internal abstract Task EnsureDatabaseExists();
        internal abstract Task EnsureCreatedAsync();
        internal abstract IEffectivePermissionsAware CreateHouseDbContext();
    }

    internal sealed class MySqlDbAggregator : DbAggregator
    {
        public const string NAME = "mysql";
        private readonly TestMySqlConfig settings;
        private readonly MySqlObacStorageProvider dbProvider;
        private readonly ObjectStorage dbStorage;
        private readonly HouseTestMySqlDbContext houseDbContext;

        private static readonly MySqlDbAggregator instance = new();

        public static MySqlDbAggregator GetInstance()
        {
            return instance;
        }

        private MySqlDbAggregator()
        {
            settings = Setting.Mysql;

            // create config for db
            dbProvider = new MySqlObacStorageProvider(settings.Config.Connection);
            dbStorage = new ObjectStorage(dbProvider);
            houseDbContext = new HouseTestMySqlDbContext(settings.ConnectionTest);
        }

        public override string Name => NAME;

        public override IObacStorageProvider DbProvider => dbProvider;
        public override ObjectStorage DbStorage => dbStorage;
        public override HouseTestDbContext HouseDbContext => houseDbContext;

        private static void CleanDb(string connectionString)
        {
            MySqlConnection connection;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("unknown database"))
                    return; // no need to drop anything :)
                throw;
            }

            var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {connection.Database};";
            command.ExecuteNonQuery();

            command.CommandText = $"CREATE DATABASE {connection.Database};";
            command.ExecuteNonQuery();
        }

        internal override void TestCoreDbCleaner()
        {
            CleanDb(settings.Config.Connection);
        }

        internal override void TestUserDbCleaner()
        {
            CleanDb(settings.ConnectionTest);
        }

        internal override Task EnsureDatabaseExists()
        {
            return dbProvider.EnsureDatabaseExists();
        }

        internal override Task EnsureCreatedAsync()
        {
            return Task.Run(() => houseDbContext.Database.EnsureCreated());
        }

        internal override IEffectivePermissionsAware CreateHouseDbContext()
        {
            return new HouseTestMySqlDbContext(settings.ConnectionTest);
        }
    }

    internal sealed class PgDbAggregator : DbAggregator
    {
        public const string NAME = "postgres";
        private readonly TestPgSqlConfig settings;
        private readonly PgSqlObacStorageProvider dbProvider;
        private readonly ObjectStorage dbStorage;
        private readonly HouseTestPgDbContext houseDbContext;

        private static readonly PgDbAggregator instance = new();

        public static PgDbAggregator GetInstance()
        {
            return instance;
        }

        private PgDbAggregator()
        {
            settings = Setting.Postgres;

            // create config for db
            dbProvider = new PgSqlObacStorageProvider(settings.Config.Connection);
            dbStorage = new ObjectStorage(dbProvider);
            houseDbContext = new HouseTestPgDbContext(settings.ConnectionTest);
        }

        public override string Name => NAME;

        public override IObacStorageProvider DbProvider => dbProvider;
        public override ObjectStorage DbStorage => dbStorage;
        public override HouseTestDbContext HouseDbContext => houseDbContext;

        private static void CleanDb(string connectionString)
        {
            NpgsqlConnection connection;
            try
            {
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
            }
            catch (PostgresException ex)
            {
                if (ex.MessageText.ToLower().Contains("does not exist"))
                    return; // no need to drop anything :)
                throw;
            }

            var command = connection.CreateCommand();
            command.CommandText = "drop schema if exists public cascade;";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE SCHEMA public;";
            command.ExecuteNonQuery();
        }

        internal override void TestCoreDbCleaner()
        {
            CleanDb(settings.Config.Connection);
        }

        internal override void TestUserDbCleaner()
        {
            CleanDb(settings.ConnectionTest);
        }

        internal override Task EnsureDatabaseExists()
        {
            return dbProvider.EnsureDatabaseExists();
        }

        internal override Task EnsureCreatedAsync()
        {
            return Task.Run(() => houseDbContext.Database.EnsureCreated());
        }

        internal override IEffectivePermissionsAware CreateHouseDbContext()
        {
            return new HouseTestPgDbContext(settings.ConnectionTest);
        }
    }

}
