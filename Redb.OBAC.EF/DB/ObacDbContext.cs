using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF.DB.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Redb.OBAC.EF.DB
{
    public abstract class ObacDbContext : DbContext
    {
        protected readonly string ConnectionString;        
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

        public abstract string GetTreeSubnodesDeepQueryRoot();

        public abstract string GetTreeSubnodesDeepQueryGivenNode();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ObacDbConfiguration.ConfigureModel(builder);
        }
    }
}