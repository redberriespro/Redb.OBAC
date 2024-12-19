using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;

namespace TreeExample
{
    internal class TreeDbContext : ObacEpContextBase
    {
        public DbSet<CatalogEntity> Catalogs { get; set; }
        public DbSet<FileEntity> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(Program.TEST_CONNECTION);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FileEntity>()
                .HasKey(a => new { a.Id, a.Version });
        }
    }
}