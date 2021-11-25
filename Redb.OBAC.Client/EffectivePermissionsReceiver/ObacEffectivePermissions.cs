using Microsoft.EntityFrameworkCore;

namespace Redb.OBAC.Client.EffectivePermissionsReceiver
{
    public static class ObacEffectivePermissions
    {
        public static void ConfigureModel(ModelBuilder builder)
        {
            builder.Entity<ObacEffectivePermissionsEntity>()
                .HasIndex(p => new {p.UserId, p.PermissionId, p.ObjectTypeId, p.ObjectId})
                .IsUnique();

            // index for obtaining all the permissions for certain user+object
            builder.Entity<ObacEffectivePermissionsEntity>()
                .HasIndex(p => new {p.UserId, p.ObjectTypeId, p.ObjectId});

            // index for obtaining all the permissions for object 
            builder.Entity<ObacEffectivePermissionsEntity>()
                .HasIndex(p => new {p.ObjectTypeId, p.ObjectId});
        }
    }
}