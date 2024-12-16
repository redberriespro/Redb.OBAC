using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;

namespace HelloObac;

public class HelloIdentityDbContext : ObacEpIdentityContextBase<IdentityUser<int>, IdentityRole<int>, int>, IHelloDbContext
{
    public DbSet<DocumentEntity> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(Program.TEST_CONNECTION_IDENTITY);
        }
    }
}