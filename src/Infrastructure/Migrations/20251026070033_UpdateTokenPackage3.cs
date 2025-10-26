using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenPackage3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "UserTokenPackages",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCode",
                table: "UserTokenPackages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimeEnd",
                table: "UserTokenPackages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TokenPackages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "UserTokenPackages");

            migrationBuilder.DropColumn(
                name: "PaymentCode",
                table: "UserTokenPackages");

            migrationBuilder.DropColumn(
                name: "TimeEnd",
                table: "UserTokenPackages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TokenPackages");
        }
    }
}
