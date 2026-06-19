using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeToStockOps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "stock_transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "stock_receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "stock_audits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "stock_adjustments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "pick_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "pack_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_EmployeeId",
                table: "stock_transfers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_receipts_EmployeeId",
                table: "stock_receipts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_audits_EmployeeId",
                table: "stock_audits",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_adjustments_EmployeeId",
                table: "stock_adjustments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_pick_orders_EmployeeId",
                table: "pick_orders",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_pack_orders_EmployeeId",
                table: "pack_orders",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_pack_orders_employees_EmployeeId",
                table: "pack_orders",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pick_orders_employees_EmployeeId",
                table: "pick_orders",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_adjustments_employees_EmployeeId",
                table: "stock_adjustments",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_audits_employees_EmployeeId",
                table: "stock_audits",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_receipts_employees_EmployeeId",
                table: "stock_receipts",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_transfers_employees_EmployeeId",
                table: "stock_transfers",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pack_orders_employees_EmployeeId",
                table: "pack_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_pick_orders_employees_EmployeeId",
                table: "pick_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_adjustments_employees_EmployeeId",
                table: "stock_adjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_audits_employees_EmployeeId",
                table: "stock_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_receipts_employees_EmployeeId",
                table: "stock_receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_transfers_employees_EmployeeId",
                table: "stock_transfers");

            migrationBuilder.DropIndex(
                name: "IX_stock_transfers_EmployeeId",
                table: "stock_transfers");

            migrationBuilder.DropIndex(
                name: "IX_stock_receipts_EmployeeId",
                table: "stock_receipts");

            migrationBuilder.DropIndex(
                name: "IX_stock_audits_EmployeeId",
                table: "stock_audits");

            migrationBuilder.DropIndex(
                name: "IX_stock_adjustments_EmployeeId",
                table: "stock_adjustments");

            migrationBuilder.DropIndex(
                name: "IX_pick_orders_EmployeeId",
                table: "pick_orders");

            migrationBuilder.DropIndex(
                name: "IX_pack_orders_EmployeeId",
                table: "pack_orders");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "stock_audits");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "stock_adjustments");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "pick_orders");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "pack_orders");
        }
    }
}
