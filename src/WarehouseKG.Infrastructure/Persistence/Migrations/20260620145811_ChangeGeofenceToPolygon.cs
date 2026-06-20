using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseKG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGeofenceToPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CenterLatitude",
                table: "geofences");

            migrationBuilder.DropColumn(
                name: "CenterLongitude",
                table: "geofences");

            migrationBuilder.DropColumn(
                name: "RadiusMeters",
                table: "geofences");

            migrationBuilder.AddColumn<string>(
                name: "Vertices",
                table: "geofences",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vertices",
                table: "geofences");

            migrationBuilder.AddColumn<double>(
                name: "CenterLatitude",
                table: "geofences",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CenterLongitude",
                table: "geofences",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RadiusMeters",
                table: "geofences",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
