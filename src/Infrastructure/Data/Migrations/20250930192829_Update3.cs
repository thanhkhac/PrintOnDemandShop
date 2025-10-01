using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_DomainUsers_CreatedByUserId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DomainUsers_CreatedByUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedByUserId",
                table: "ProductDesign");

            migrationBuilder.DropIndex(
                name: "IX_ProductDesign_CreatedByUserId",
                table: "ProductDesign");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedByUserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CreatedByUserId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ProductDesign");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesign_CreatedBy",
                table: "ProductDesign",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedBy",
                table: "Orders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CreatedBy",
                table: "CartItems",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_DomainUsers_CreatedBy",
                table: "CartItems",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DomainUsers_CreatedBy",
                table: "Orders",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedBy",
                table: "ProductDesign",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_DomainUsers_CreatedBy",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DomainUsers_CreatedBy",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedBy",
                table: "ProductDesign");

            migrationBuilder.DropIndex(
                name: "IX_ProductDesign_CreatedBy",
                table: "ProductDesign");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedBy",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CreatedBy",
                table: "CartItems");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ProductDesign",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "CartItems",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesign_CreatedByUserId",
                table: "ProductDesign",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByUserId",
                table: "Orders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CreatedByUserId",
                table: "CartItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_DomainUsers_CreatedByUserId",
                table: "CartItems",
                column: "CreatedByUserId",
                principalTable: "DomainUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DomainUsers_CreatedByUserId",
                table: "Orders",
                column: "CreatedByUserId",
                principalTable: "DomainUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedByUserId",
                table: "ProductDesign",
                column: "CreatedByUserId",
                principalTable: "DomainUsers",
                principalColumn: "Id");
        }
    }
}
