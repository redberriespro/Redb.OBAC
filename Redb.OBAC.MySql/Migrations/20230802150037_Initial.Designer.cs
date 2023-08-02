﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Redb.OBAC.MySql;

namespace Redb.OBAC.MySql.Migrations
{
    [DbContext(typeof(MySqlObacDbContext))]
    [Migration("20230802150037_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacGroupSubjectEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<int?>("ExternalIdInt")
                        .HasColumnType("int")
                        .HasColumnName("external_id_int");

                    b.Property<string>("ExternalIdString")
                        .HasColumnType("longtext")
                        .HasColumnName("external_id_str");

                    b.HasKey("Id");

                    b.ToTable("obac_user_groups");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacObjectTypeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<int>("Type")
                        .HasColumnType("int")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.ToTable("obac_objecttypes");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacPermissionEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.HasKey("Id");

                    b.ToTable("obac_permissions");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacPermissionRoleEntity", b =>
                {
                    b.Property<Guid>("PermissionId")
                        .HasColumnType("char(36)")
                        .HasColumnName("perm_id");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("char(36)")
                        .HasColumnName("role_id");

                    b.HasKey("PermissionId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("obac_permissions_in_roles");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacRoleEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.HasKey("Id");

                    b.ToTable("obac_roles");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacTreeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<int?>("ExternalIdInt")
                        .HasColumnType("int")
                        .HasColumnName("external_id_int");

                    b.Property<string>("ExternalIdString")
                        .HasColumnType("longtext")
                        .HasColumnName("external_id_str");

                    b.HasKey("Id");

                    b.ToTable("obac_trees");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacTreeNodeEntity", b =>
                {
                    b.Property<Guid>("TreeId")
                        .HasColumnType("char(36)")
                        .HasColumnName("tree_id");

                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Acl")
                        .HasColumnType("longtext")
                        .HasColumnName("acl");

                    b.Property<int?>("ExternalIdInt")
                        .HasColumnType("int")
                        .HasColumnName("external_id_int");

                    b.Property<string>("ExternalIdString")
                        .HasColumnType("longtext")
                        .HasColumnName("external_id_str");

                    b.Property<bool>("InheritParentPermissions")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("inherit_parent_perms");

                    b.Property<int>("OwnerUserId")
                        .HasColumnType("int")
                        .HasColumnName("owner_user_id");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int")
                        .HasColumnName("parent_id");

                    b.HasKey("TreeId", "Id");

                    b.HasIndex("OwnerUserId");

                    b.HasIndex("TreeId");

                    b.HasIndex("TreeId", "ParentId");

                    b.HasIndex("TreeId", "Id", "ParentId");

                    b.ToTable("obac_tree_nodes");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacTreeNodePermissionEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<bool>("Deny")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_deny");

                    b.Property<int>("NodeId")
                        .HasColumnType("int")
                        .HasColumnName("tree_node_id");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("char(36)")
                        .HasColumnName("perm_id");

                    b.Property<Guid>("TreeId")
                        .HasColumnType("char(36)")
                        .HasColumnName("tree_id");

                    b.Property<int?>("UserGroupId")
                        .HasColumnType("int")
                        .HasColumnName("user_group_id");

                    b.Property<int?>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserGroupId");

                    b.HasIndex("TreeId", "NodeId");

                    b.HasIndex("TreeId", "NodeId", "PermissionId");

                    b.HasIndex("UserId", "UserGroupId", "TreeId", "NodeId", "PermissionId")
                        .IsUnique();

                    b.ToTable("obac_tree_node_permissions");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacUserInGroupEntity", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.Property<int>("GroupId")
                        .HasColumnType("int")
                        .HasColumnName("group_id");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("obac_users_in_groups");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacUserPermissionsEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)")
                        .HasColumnName("id");

                    b.Property<int?>("ObjectId")
                        .HasColumnType("int")
                        .HasColumnName("objid");

                    b.Property<Guid>("ObjectTypeId")
                        .HasColumnType("char(36)")
                        .HasColumnName("objtypeid");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("char(36)")
                        .HasColumnName("permid");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("userid");

                    b.HasKey("Id");

                    b.HasIndex("ObjectTypeId", "ObjectId");

                    b.HasIndex("UserId", "ObjectTypeId", "ObjectId");

                    b.HasIndex("UserId", "PermissionId", "ObjectTypeId", "ObjectId")
                        .IsUnique();

                    b.ToTable("obac_userpermissions");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacUserSubjectEntity", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<int?>("ExternalIdInt")
                        .HasColumnType("int")
                        .HasColumnName("external_id_int");

                    b.Property<string>("ExternalIdString")
                        .HasColumnType("varchar(255)")
                        .HasColumnName("external_id_str");

                    b.HasKey("Id");

                    b.HasIndex("ExternalIdInt");

                    b.HasIndex("ExternalIdString");

                    b.ToTable("obac_users");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacPermissionRoleEntity", b =>
                {
                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacRoleEntity", "Role")
                        .WithMany("Permissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacTreeNodeEntity", b =>
                {
                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacUserSubjectEntity", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacTreeEntity", "Tree")
                        .WithMany()
                        .HasForeignKey("TreeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacTreeNodeEntity", "Parent")
                        .WithMany()
                        .HasForeignKey("TreeId", "ParentId");

                    b.Navigation("Owner");

                    b.Navigation("Parent");

                    b.Navigation("Tree");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacTreeNodePermissionEntity", b =>
                {
                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacGroupSubjectEntity", "UserGroup")
                        .WithMany()
                        .HasForeignKey("UserGroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacUserSubjectEntity", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacTreeNodeEntity", "Node")
                        .WithMany()
                        .HasForeignKey("TreeId", "NodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Node");

                    b.Navigation("User");

                    b.Navigation("UserGroup");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacUserInGroupEntity", b =>
                {
                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacGroupSubjectEntity", "Group")
                        .WithMany("Users")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Redb.OBAC.EF.DB.Entities.ObacUserSubjectEntity", "User")
                        .WithMany("Groups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacGroupSubjectEntity", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacRoleEntity", b =>
                {
                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("Redb.OBAC.EF.DB.Entities.ObacUserSubjectEntity", b =>
                {
                    b.Navigation("Groups");
                });
#pragma warning restore 612, 618
        }
    }
}