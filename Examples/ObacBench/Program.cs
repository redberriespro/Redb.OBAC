using System;
using HelloObac;
using Redb.OBAC;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.PgSql;

Console.WriteLine("Bench Test");

const string OBAC_CONNECTION =
    "Host=localhost;Port=5432;Database=obac;Username=postgres;Password=12345678";

// initialize local DB
var ctx = new HelloDbContext(); // the context must inherit ObacEpContextBase to be able to receive EP messages.
await ctx.Database.EnsureCreatedAsync();
            
// configure OBAC
var pgStorage = new PgSqlObacStorageProvider(OBAC_CONNECTION);
await pgStorage.EnsureDatabaseExists();
            
// NOTE the context instance passed to the receiver could not be used across other program when in production code
var epHouseReceiver = new EffectivePermissionsEfReceiver(ctx); 
            
// initialize OBAC with out effective permission's receiver
var obacConfiguration = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);

var obacManager = obacConfiguration.GetObjectManager();

// todo