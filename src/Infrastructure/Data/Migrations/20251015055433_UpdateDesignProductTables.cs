using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDesignProductTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "ProductDesigns",
                newName: "ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProductDesigns",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductDesignIcons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductDesignId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImageUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDesignIcons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDesignIcons_ProductDesigns_ProductDesignId",
                        column: x => x.ProductDesignId,
                        principalTable: "ProductDesigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductDesignTemplates",
                columns: table => new
                {
                    ProductDesignId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DesignImageUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrintAreaName = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDesignTemplates", x => new { x.ProductDesignId, x.TemplateId });
                    table.ForeignKey(
                        name: "FK_ProductDesignTemplates_ProductDesigns_ProductDesignId",
                        column: x => x.ProductDesignId,
                        principalTable: "ProductDesigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductDesignTemplates_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesigns_ProductId",
                table: "ProductDesigns",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesigns_ProductOptionValueId",
                table: "ProductDesigns",
                column: "ProductOptionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesignIcons_ProductDesignId",
                table: "ProductDesignIcons",
                column: "ProductDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesignTemplates_TemplateId",
                table: "ProductDesignTemplates",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesigns_ProductOptionValues_ProductOptionValueId",
                table: "ProductDesigns",
                column: "ProductOptionValueId",
                principalTable: "ProductOptionValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesigns_Products_ProductId",
                table: "ProductDesigns",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesigns_ProductOptionValues_ProductOptionValueId",
                table: "ProductDesigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesigns_Products_ProductId",
                table: "ProductDesigns");

            migrationBuilder.DropTable(
                name: "ProductDesignIcons");

            migrationBuilder.DropTable(
                name: "ProductDesignTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ProductDesigns_ProductId",
                table: "ProductDesigns");

            migrationBuilder.DropIndex(
                name: "IX_ProductDesigns_ProductOptionValueId",
                table: "ProductDesigns");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProductDesigns");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductDesigns",
                newName: "TemplateId");
        }
    }
}
