using Microsoft.EntityFrameworkCore;
using Redb.OBAC.EF.DB.Entities;

namespace Redb.OBAC.EF.DB
{
    public static class ObacDbConfiguration
    {
        public static void ConfigureModel(ModelBuilder builder)
        {
            builder.Entity<ObacUserSubjectEntity>()
                .HasIndex(p => p.ExternalIdInt);

            builder.Entity<ObacUserSubjectEntity>()
                .HasIndex(p => p.ExternalIdString);

            builder.Entity<ObacUserPermissionsEntity>()
                .HasIndex(p => new { p.UserId, p.PermissionId, p.ObjectTypeId, p.ObjectId })
                .IsUnique();

            // index for obtaining all the permissions for certain user+object
            builder.Entity<ObacUserPermissionsEntity>()
                .HasIndex(p => new { p.UserId, p.ObjectTypeId, p.ObjectId });

            // index for obtaining all the permissions for object 
            builder.Entity<ObacUserPermissionsEntity>()
                .HasIndex(p => new { p.ObjectTypeId, p.ObjectId });

            builder.Entity<ObacPermissionRoleEntity>()
                .HasKey(a => new { a.PermissionId, a.RoleId });

            builder.Entity<ObacPermissionRoleEntity>()
                .HasOne(p => p.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(p => p.RoleId);


            // trees

            builder.Entity<ObacTreeNodeEntity>()
                .HasIndex(p => new { p.TreeId, p.Id, p.ParentId });

            builder.Entity<ObacTreeNodeEntity>()
                .HasKey(p => new { p.TreeId, p.Id });

            builder.Entity<ObacTreeNodeEntity>()
                .HasIndex(p => p.TreeId);

            builder.Entity<ObacTreeNodeEntity>()
                .HasOne(p => p.Tree)
                .WithMany()
                .HasForeignKey(p => p.TreeId);

            builder.Entity<ObacTreeNodeEntity>()
                .HasOne(p => p.Parent)
                .WithMany()
                .HasForeignKey(p => new { p.TreeId, p.ParentId });

            builder.Entity<ObacTreeNodeEntity>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerUserId);

            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasIndex(k => new { k.UserId, k.UserGroupId, k.TreeId, k.NodeId, k.PermissionId })
                .IsUnique();

            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasIndex(k => new { k.TreeId, k.NodeId });

            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasIndex(k => new { k.UserGroupId }); // to easy rebuild when group members changes

            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasIndex(k => new { k.TreeId, k.NodeId, k.PermissionId });

            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasOne(p => p.UserGroup)
                .WithMany()
                .HasForeignKey(p => p.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<ObacTreeNodePermissionEntity>()
                .HasOne(p => p.Node)
                .WithMany()
                .HasForeignKey(p => new { p.TreeId, p.NodeId })
                .OnDelete(DeleteBehavior.Cascade);



            // userGroups
            builder.Entity<ObacUserInGroupEntity>().HasKey(x => new { x.UserId, x.GroupId });
            builder.Entity<ObacUserInGroupEntity>().HasIndex(x => x.UserId);
            builder.Entity<ObacUserInGroupEntity>().HasIndex(x => x.GroupId);

            builder.Entity<ObacUserInGroupEntity>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.Groups)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade); ;

            builder.Entity<ObacUserInGroupEntity>()
                .HasOne(pt => pt.Group)
                .WithMany(p => p.Users)
                .HasForeignKey(pt => pt.GroupId)
                .OnDelete(DeleteBehavior.Cascade); ;


        }
    }
}