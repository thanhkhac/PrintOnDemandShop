using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSoftDeleteTemlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrintArea",
                table: "Templates",
                newName: "PrintAreaName");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Templates",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Templates",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Templates");

            migrationBuilder.RenameColumn(
                name: "PrintAreaName",
                table: "Templates",
                newName: "PrintArea");
        }
    }
}
