using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeToPOandSO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "sales_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "purchase_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_EmployeeId",
                table: "sales_orders",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_EmployeeId",
                table: "purchase_orders",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_orders_employees_EmployeeId",
                table: "purchase_orders",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_orders_employees_EmployeeId",
                table: "sales_orders",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_orders_employees_EmployeeId",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_orders_employees_EmployeeId",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_sales_orders_EmployeeId",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_purchase_orders_EmployeeId",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "purchase_orders");
        }
    }
}
