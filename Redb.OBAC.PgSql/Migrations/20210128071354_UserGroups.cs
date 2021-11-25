using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class UserGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "obac_user_groups",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id_int = table.Column<int>(nullable: true),
                    external_id_str = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_user_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_users_in_groups",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_users_in_groups", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "FK_obac_users_in_groups_obac_user_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "obac_user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_users_in_groups_obac_users_user_id",
                        column: x => x.user_id,
                        principalTable: "obac_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_in_groups_group_id",
                table: "obac_users_in_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_in_groups_user_id",
                table: "obac_users_in_groups",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "obac_users_in_groups");

            migrationBuilder.DropTable(
                name: "obac_user_groups");
        }
    }
}
