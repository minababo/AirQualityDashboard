using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirQualityDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertThresholdSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertThresholdSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PM25Threshold = table.Column<float>(type: "real", nullable: false),
                    PM10Threshold = table.Column<float>(type: "real", nullable: false),
                    RHThreshold = table.Column<float>(type: "real", nullable: false),
                    TempThreshold = table.Column<float>(type: "real", nullable: false),
                    WindThreshold = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertThresholdSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertThresholdSettings");
        }
    }
}
