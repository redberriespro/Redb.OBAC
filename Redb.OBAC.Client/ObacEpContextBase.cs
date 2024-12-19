using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client.EffectivePermissionsReceiver;

namespace Redb.OBAC.Client
{
    public abstract class ObacEpContextBase: DbContext, IObacEpContext
    {
        protected ObacEpContextBase()
        {
        }

        protected ObacEpContextBase(DbContextOptions options) : base(options)
        {
        }
        

        public DbSet<ObacEffectivePermissionsEntity> EffectivePermissions { get; set; }
            
        Task IEffectivePermissionsAware.SaveChangesAsync()
        {
            return SaveChangesAsync();
        }

        public async Task DropEffectivePermissions(Guid objectTypeId, Guid objectId)
        {
            await Database.ExecuteSqlRawAsync( 
                "DELETE FROM obac_ep WHERE objtypeid = {0} and objid = {1}", 
                objectTypeId, objectId); 

        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
                
            // Obac EP Receiver Support 
            ObacEffectivePermissions.ConfigureModel(builder);
        }
    }
}