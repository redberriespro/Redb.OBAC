using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using Redb.OBAC.ApiHost;
using Redb.OBAC.EF.BL;
using Redb.OBAC.Client;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.EF.DB;
using Redb.OBAC.MsSql;
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
using Redb.OBAC.EF;
using Redb.OBAC.MongoDriver.DB;
using Redb.OBAC.MongoDb;
using MongoDB.Driver;
using Redb.OBAC.Tree;
using Redb.OBAC.Core;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Redb.OBAC.Tests.Utils
{
    internal abstract class DbAggregator
    {
        protected bool isInit = false;
        protected static TestAppSettings Setting;
        protected IEffectivePermissionFeed epHouseReceiver;
        protected IObacConfiguration config;
        private ApiHostImpl apiHost;


        public DbAggregator()
        {
            if (Setting == null) Setting = ReadSettings().Result;
        }


        public IObacConfiguration Configuration => config;
        public ApiHostImpl ApiHost => apiHost;

        public abstract string Name { get; }

        //public abstract EF.DB.IObacStorageProvider DbProvider { get; }
        public abstract IObjectStorage DbStorage { get; }
        public abstract IEffectivePermissionsAware HouseDbContext { get; }


        protected static async Task<TestAppSettings> ReadSettings()
        {
            var appBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFile = Path.Combine(appBinPath, "appsettings.json");
            var configText = await File.ReadAllTextAsync(configFile);
            return JsonConvert.DeserializeObject<TestAppSettings>(configText);
        }

        public abstract Task InitTestFramework();

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

    internal sealed class MsSqlDbAggregator : DbAggregator
    {
        public const string NAME = "mssql";
        private readonly TestMsSqlConfig settings;
        private readonly MsSqlObacStorageProvider dbProvider;
        private readonly ObjectStorage dbStorage;
        private readonly HouseTestMsSqlDbContext houseDbContext;

        private static readonly MsSqlDbAggregator instance = new();

        public static MsSqlDbAggregator GetInstance()
        {
            return instance;
        }

        private MsSqlDbAggregator()
        {
            settings = Setting.Mssql;

            // create config for db
            dbProvider = new MsSqlObacStorageProvider(settings.Config.Connection);
            dbStorage = new ObjectStorage(dbProvider);
            houseDbContext = new HouseTestMsSqlDbContext(settings.ConnectionTest);
        }

        public override string Name => NAME;

        public EF.DB.IObacStorageProvider DbProvider => dbProvider;
        public override ObjectStorage DbStorage => dbStorage;
        public override HouseTestDbContext HouseDbContext => houseDbContext;

        public override async Task InitTestFramework()
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
            if (config == null) config = Redb.OBAC.EF.ObacManager.CreateConfiguration(DbProvider, epHouseReceiver);

            // initialize in-process obac api based on db config
            CreateApiHost();
        }

        private static void CleanDb(string connectionString)
        {
            SqlConnection connection;
            var databaseName = string.Empty;
            try
            {
                connection = new SqlConnection(connectionString);
                databaseName = connection.Database;
                connection.Open();
            }
            catch (SqlException ex)
            {
                if (ex.ErrorCode == -2146232060)
                    return; // no need to drop anything :)
                throw;
            }
            connection.ChangeDatabase("master");
            var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {databaseName};";
            command.ExecuteNonQuery();

            command.CommandText = $"CREATE DATABASE {databaseName};";
            command.ExecuteNonQuery();
            command.Dispose();
            connection.Close();
            connection.Dispose();
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

        internal override IObacEpContext CreateHouseDbContext()
        {
            return new HouseTestMsSqlDbContext(settings.ConnectionTest);
        }
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

        public EF.DB.IObacStorageProvider DbProvider => dbProvider;
        public override ObjectStorage DbStorage => dbStorage;
        public override HouseTestDbContext HouseDbContext => houseDbContext;

        public override async Task InitTestFramework()
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
            if (config == null) config = Redb.OBAC.EF.ObacManager.CreateConfiguration(DbProvider, epHouseReceiver);

            // initialize in-process obac api based on db config
            CreateApiHost();
        }
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

        internal override IObacEpContext CreateHouseDbContext()
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

        public EF.DB.IObacStorageProvider DbProvider => dbProvider;
        public override ObjectStorage DbStorage => dbStorage;
        public override HouseTestDbContext HouseDbContext => houseDbContext;

        public override async Task InitTestFramework()
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
            if (config == null) config = Redb.OBAC.EF.ObacManager.CreateConfiguration(DbProvider, epHouseReceiver);

            // initialize in-process obac api based on db config
            CreateApiHost();
        }

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
                if (ex.Code == "3D000")
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

        internal override IObacEpContext CreateHouseDbContext()
        {
            return new HouseTestPgDbContext(settings.ConnectionTest);
        }
    }

    internal sealed class MongoDbAggregator : DbAggregator
    {
        public const string NAME = "mongodb";
        private readonly TestMongoDbConfig settings;
        private readonly MongoDbObacStorageProvider dbProvider;
        private readonly MongoDriver.BL.ObjectStorage dbStorage;
        private readonly HouseTestMongoDbContext houseDbContext;

        private static readonly MongoDbAggregator instance = new();

        public static MongoDbAggregator GetInstance()
        {
            return instance;
        }

        private MongoDbAggregator()
        {
            settings = Setting.MongoDb;

            // create config for db
            dbProvider = new MongoDbObacStorageProvider(settings.Config.Connection);
            dbStorage = new MongoDriver.BL.ObjectStorage(dbProvider);
            houseDbContext = new HouseTestMongoDbContext(settings.ConnectionTest);
        }

        public override string Name => NAME;

        public MongoDriver.DB.IObacStorageProvider DbProvider => dbProvider;
        public override MongoDriver.BL.ObjectStorage DbStorage => dbStorage;
        public override IEffectivePermissionsAware HouseDbContext => houseDbContext;

        public override async Task InitTestFramework()
        {
            if (isInit) return;
            isInit = true;

            // drop DBs before running any tests
            TestCoreDbCleaner();
            await EnsureDatabaseExists();

            TestUserDbCleaner();
            await EnsureCreatedAsync();

            epHouseReceiver = new MongoDbClient.EffectivePermissionsReceiver.EffectivePermissionsEfReceiver(CreateHouseDbContext());

            // register OBAC configuration
            if (config == null) config = Redb.OBAC.MongoDriver.ObacManager.CreateConfiguration(DbProvider, epHouseReceiver);

            // initialize in-process obac api based on db config
            CreateApiHost();
        }

        private static void CleanDb(string connectionString)
        {
            MongoClient connection;
            var url = new MongoUrl(connectionString);
            var databaseName = string.Empty;

            connection = new MongoClient(connectionString);
            connection.DropDatabase(url.DatabaseName);

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
            return Task.Run(() => houseDbContext.EnsureCreated());
        }

        internal override IEffectivePermissionsAware CreateHouseDbContext()
        {
            return new HouseTestMongoDbContext(settings.ConnectionTest);
        }
    }

}
