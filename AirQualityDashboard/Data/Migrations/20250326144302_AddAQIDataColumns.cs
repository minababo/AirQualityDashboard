using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirQualityDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAQIDataColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AQI",
                table: "AQIData");

            migrationBuilder.AddColumn<double>(
                name: "PM10",
                table: "AQIData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PM25",
                table: "AQIData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PM10",
                table: "AQIData");

            migrationBuilder.DropColumn(
                name: "PM25",
                table: "AQIData");

            migrationBuilder.AddColumn<int>(
                name: "AQI",
                table: "AQIData",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
