using Microsoft.EntityFrameworkCore;
using Redb.OBAC.EF.DB.Entities;

namespace Redb.OBAC.EF.DB
{
    public enum DbType
    {
        MsSql,
        MySql,
        PgSQL
    }
    public abstract class ObacDbContext : DbContext
    {
        protected readonly string ConnectionString;
        public DbType DbType { get; protected set; } = DbType.PgSQL;
        public ObacDbContext(DbContextOptions<ObacDbContext> options) : base(options)
        {
        }

        public ObacDbContext() : base()
        {
        }

        public ObacDbContext(string connectionString) : this()
        {
            ConnectionString = connectionString;
        }

        public DbSet<ObacPermissionEntity> ObacPermissions { get; set; }
        public DbSet<ObacRoleEntity> ObacRoles { get; set; }
        public DbSet<ObacObjectTypeEntity> ObacObjectTypes { get; set; }
        public DbSet<ObacUserSubjectEntity> ObacUserSubjects { get; set; }
        public DbSet<ObacGroupSubjectEntity> ObacGroupSubjects { get; set; }
        public DbSet<ObacUserInGroupEntity> ObacUsersInGroups { get; set; }

        public DbSet<ObacTreeEntity> ObacTree { get; set; }
        public DbSet<ObacTreeNodeEntity> ObacTreeNodes { get; set; }
        public DbSet<ObacTreeNodePermissionEntity> ObacTreeNodePermissions { get; set; }

        public DbSet<ObacUserPermissionsEntity> ObacUserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ObacDbConfiguration.ConfigureModel(builder);
        }
    }
}