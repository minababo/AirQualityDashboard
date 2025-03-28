using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirQualityDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCO2WithOtherMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CO2",
                table: "AQIData",
                newName: "Wind");

            migrationBuilder.AddColumn<double>(
                name: "PM1",
                table: "AQIData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RH",
                table: "AQIData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Temp",
                table: "AQIData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PM1",
                table: "AQIData");

            migrationBuilder.DropColumn(
                name: "RH",
                table: "AQIData");

            migrationBuilder.DropColumn(
                name: "Temp",
                table: "AQIData");

            migrationBuilder.RenameColumn(
                name: "Wind",
                table: "AQIData",
                newName: "CO2");
        }
    }
}
