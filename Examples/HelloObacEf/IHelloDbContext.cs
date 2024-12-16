using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;

namespace HelloObac;

public interface IHelloDbContext : IObacEpContext
{
    DbSet<DocumentEntity> Documents { get; set; }
}