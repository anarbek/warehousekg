using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "item_categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_ParentId",
                table: "item_categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_TenantId_ParentId",
                table: "item_categories",
                columns: new[] { "TenantId", "ParentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_item_categories_item_categories_ParentId",
                table: "item_categories",
                column: "ParentId",
                principalTable: "item_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_categories_item_categories_ParentId",
                table: "item_categories");

            migrationBuilder.DropIndex(
                name: "IX_item_categories_ParentId",
                table: "item_categories");

            migrationBuilder.DropIndex(
                name: "IX_item_categories_TenantId_ParentId",
                table: "item_categories");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "item_categories");
        }
    }
}
