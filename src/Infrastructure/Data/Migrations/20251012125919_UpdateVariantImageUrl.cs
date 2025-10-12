using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVariantImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "DiscountPercent",
                table: "Vouchers",
                newName: "DiscountValue");

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Vouchers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductVariants",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductVariants");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                table: "Vouchers",
                newName: "DiscountPercent");

            migrationBuilder.AddColumn<long>(
                name: "DiscountAmount",
                table: "Vouchers",
                type: "bigint",
                nullable: true);
        }
    }
}
