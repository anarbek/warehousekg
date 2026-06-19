using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DefaultCapacityKg = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    DefaultCapacityM3 = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VIN = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Brand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: true),
                    VehicleTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnershipType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FuelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FuelConsumptionRate = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MaxCapacityKg = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    MaxCapacityM3 = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    CurrentMileageKm = table.Column<decimal>(type: "numeric(18,1)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InsuranceProvider = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TechInspectionExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextMaintenanceMileageKm = table.Column<decimal>(type: "numeric(18,1)", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasGpsTracker = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_vehicle_types_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "vehicle_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_driver_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_driver_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_driver_assignments_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vehicle_driver_assignments_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_fuel_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Liters = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MileageKm = table.Column<decimal>(type: "numeric(18,1)", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Station = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_fuel_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_fuel_records_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_inspection_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Inspector = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_inspection_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_inspection_records_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_insurance_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Provider = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CoverageType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_insurance_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_insurance_records_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_maintenance_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MileageKm = table.Column<decimal>(type: "numeric(18,1)", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ServiceProvider = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    NextDueMileageKm = table.Column<decimal>(type: "numeric(18,1)", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_maintenance_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_maintenance_records_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_driver_assignments_EmployeeId",
                table: "vehicle_driver_assignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_driver_assignments_TenantId_VehicleId_EmployeeId_As~",
                table: "vehicle_driver_assignments",
                columns: new[] { "TenantId", "VehicleId", "EmployeeId", "AssignedFromUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_driver_assignments_VehicleId",
                table: "vehicle_driver_assignments",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_fuel_records_VehicleId",
                table: "vehicle_fuel_records",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_inspection_records_VehicleId",
                table: "vehicle_inspection_records",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_insurance_records_VehicleId",
                table: "vehicle_insurance_records",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_maintenance_records_VehicleId",
                table: "vehicle_maintenance_records",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_types_TenantId_Code",
                table: "vehicle_types",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_Code",
                table: "vehicles",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_TenantId_LicensePlate",
                table: "vehicles",
                columns: new[] { "TenantId", "LicensePlate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_VehicleTypeId",
                table: "vehicles",
                column: "VehicleTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_driver_assignments");

            migrationBuilder.DropTable(
                name: "vehicle_fuel_records");

            migrationBuilder.DropTable(
                name: "vehicle_inspection_records");

            migrationBuilder.DropTable(
                name: "vehicle_insurance_records");

            migrationBuilder.DropTable(
                name: "vehicle_maintenance_records");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_types");
        }
    }
}
