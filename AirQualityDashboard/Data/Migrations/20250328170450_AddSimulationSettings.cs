using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirQualityDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSimulationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimulationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PM25Min = table.Column<float>(type: "real", nullable: false),
                    PM25Max = table.Column<float>(type: "real", nullable: false),
                    PM10Min = table.Column<float>(type: "real", nullable: false),
                    PM10Max = table.Column<float>(type: "real", nullable: false),
                    RHMin = table.Column<float>(type: "real", nullable: false),
                    RHMax = table.Column<float>(type: "real", nullable: false),
                    TempMin = table.Column<float>(type: "real", nullable: false),
                    TempMax = table.Column<float>(type: "real", nullable: false),
                    WindMin = table.Column<float>(type: "real", nullable: false),
                    WindMax = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimulationSettings");
        }
    }
}
