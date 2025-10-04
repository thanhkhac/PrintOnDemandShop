using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOptionValueImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductDesign_ProductDesignId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductDesign_ProductDesignId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedBy",
                table: "ProductDesign");

            migrationBuilder.DropTable(
                name: "ProductVariantImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDesign",
                table: "ProductDesign");

            migrationBuilder.RenameTable(
                name: "ProductDesign",
                newName: "ProductDesigns");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDesign_CreatedBy",
                table: "ProductDesigns",
                newName: "IX_ProductDesigns_CreatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductOptions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OrderItems",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDesigns",
                table: "ProductDesigns",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductOptionValueImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductOptionValueId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImageUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOptionValueImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOptionValueImages_ProductOptionValues_ProductOptionVa~",
                        column: x => x.ProductOptionValueId,
                        principalTable: "ProductOptionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOptionValueImages_ProductOptionValueId",
                table: "ProductOptionValueImages",
                column: "ProductOptionValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductDesigns_ProductDesignId",
                table: "CartItems",
                column: "ProductDesignId",
                principalTable: "ProductDesigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductDesigns_ProductDesignId",
                table: "OrderItems",
                column: "ProductDesignId",
                principalTable: "ProductDesigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesigns_DomainUsers_CreatedBy",
                table: "ProductDesigns",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductDesigns_ProductDesignId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductDesigns_ProductDesignId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesigns_DomainUsers_CreatedBy",
                table: "ProductDesigns");

            migrationBuilder.DropTable(
                name: "ProductOptionValueImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDesigns",
                table: "ProductDesigns");

            migrationBuilder.RenameTable(
                name: "ProductDesigns",
                newName: "ProductDesign");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDesigns_CreatedBy",
                table: "ProductDesign",
                newName: "IX_ProductDesign_CreatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductOptions",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OrderItems",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDesign",
                table: "ProductDesign",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductVariantImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductVariantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImageUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantImages_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImages_ProductVariantId",
                table: "ProductVariantImages",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductDesign_ProductDesignId",
                table: "CartItems",
                column: "ProductDesignId",
                principalTable: "ProductDesign",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductDesign_ProductDesignId",
                table: "OrderItems",
                column: "ProductDesignId",
                principalTable: "ProductDesign",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesign_DomainUsers_CreatedBy",
                table: "ProductDesign",
                column: "CreatedBy",
                principalTable: "DomainUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
