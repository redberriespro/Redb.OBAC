using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class ObjectTypeEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "obac_objecttypes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "obac_objecttypes");
        }
    }
}
