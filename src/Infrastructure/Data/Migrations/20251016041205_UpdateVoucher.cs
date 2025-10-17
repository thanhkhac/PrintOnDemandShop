using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "MinOrderValue",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "Vouchers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MaxDiscountAmount",
                table: "Vouchers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MinOrderValue",
                table: "Vouchers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
