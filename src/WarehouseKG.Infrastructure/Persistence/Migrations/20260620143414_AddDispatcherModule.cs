using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatcherModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresAgeVerification",
                table: "item_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "delivery_routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DriverEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_routes_employees_DriverEmployeeId",
                        column: x => x.DriverEmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_routes_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "geofences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CenterLatitude = table.Column<double>(type: "double precision", nullable: false),
                    CenterLongitude = table.Column<double>(type: "double precision", nullable: false),
                    RadiusMeters = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_stops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    PlannedArrivalUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedDepartureUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualArrivalUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualDepartureUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HasRegulatedGoods = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_stops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_stops_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_stops_delivery_routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "delivery_routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryStopId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_shipments_delivery_stops_DeliveryStopId",
                        column: x => x.DeliveryStopId,
                        principalTable: "delivery_stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_delivery_shipments_sales_orders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "sales_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_routes_DriverEmployeeId",
                table: "delivery_routes",
                column: "DriverEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_routes_TenantId_Code",
                table: "delivery_routes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_routes_TenantId_Date",
                table: "delivery_routes",
                columns: new[] { "TenantId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_routes_VehicleId",
                table: "delivery_routes",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_shipments_DeliveryStopId",
                table: "delivery_shipments",
                column: "DeliveryStopId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_shipments_SalesOrderId",
                table: "delivery_shipments",
                column: "SalesOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_stops_CustomerId",
                table: "delivery_stops",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_stops_RouteId_SequenceNumber",
                table: "delivery_stops",
                columns: new[] { "RouteId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_geofences_TenantId_Code",
                table: "geofences",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_shipments");

            migrationBuilder.DropTable(
                name: "geofences");

            migrationBuilder.DropTable(
                name: "delivery_stops");

            migrationBuilder.DropTable(
                name: "delivery_routes");

            migrationBuilder.DropColumn(
                name: "RequiresAgeVerification",
                table: "item_categories");
        }
    }
}
