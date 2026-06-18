using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "warehouses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "warehouses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "warehouses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "warehouses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "warehouse_locations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "warehouse_locations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "warehouse_locations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "warehouse_locations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "units_of_measure",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "units_of_measure",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "units_of_measure",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "units_of_measure",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TenantPermissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TenantPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TenantPermissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TenantPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "suppliers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "suppliers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_transfers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_transfers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_transfers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_transfers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_transfer_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_transfer_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_transfer_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_transfer_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_receipts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_receipts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_receipts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_receipts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_receipt_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_receipt_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_receipt_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_receipt_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_audits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_audits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_audits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_audit_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_audit_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_audit_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_audit_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_adjustments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_adjustments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_adjustments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_adjustments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "stock_adjustment_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "stock_adjustment_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "stock_adjustment_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "stock_adjustment_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "sales_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "sales_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "sales_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "sales_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "sales_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "sales_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "sales_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "sales_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "purchase_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "purchase_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "purchase_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "purchase_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "purchase_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "purchase_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "purchase_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "purchase_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "pick_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "pick_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "pick_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "pick_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "pick_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "pick_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "pick_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "pick_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "pack_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "pack_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "pack_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "pack_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "pack_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "pack_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "pack_order_lines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "pack_order_lines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "item_categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "item_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "item_categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "item_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "inventory_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "inventory_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "inventory_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "inventory_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "customers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "customers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "customers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "warehouse_locations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "warehouse_locations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "warehouse_locations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "warehouse_locations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TenantPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TenantPermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TenantPermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TenantPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_transfer_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_transfer_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_transfer_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_transfer_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_receipts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_receipt_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_receipt_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_receipt_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_receipt_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_audits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_audits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_audits");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_audits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_audit_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_audit_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_audit_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_audit_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_adjustments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_adjustments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_adjustments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_adjustments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "stock_adjustment_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "stock_adjustment_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "stock_adjustment_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "stock_adjustment_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "purchase_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "purchase_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "purchase_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "purchase_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "pick_orders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "pick_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "pick_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "pick_orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "pick_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "pick_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "pick_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "pick_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "pack_orders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "pack_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "pack_orders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "pack_orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "pack_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "pack_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "pack_order_lines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "pack_order_lines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "item_categories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "item_categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "item_categories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "item_categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "customers");
        }
    }
}
