using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductDesignId1",
                table: "ProductDesignTemplates",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDesignTemplates_ProductDesignId1",
                table: "ProductDesignTemplates",
                column: "ProductDesignId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDesignTemplates_ProductDesigns_ProductDesignId1",
                table: "ProductDesignTemplates",
                column: "ProductDesignId1",
                principalTable: "ProductDesigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductDesignTemplates_ProductDesigns_ProductDesignId1",
                table: "ProductDesignTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ProductDesignTemplates_ProductDesignId1",
                table: "ProductDesignTemplates");

            migrationBuilder.DropColumn(
                name: "ProductDesignId1",
                table: "ProductDesignTemplates");
        }
    }
}
