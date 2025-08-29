using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeverNeverLand.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prices_SeasonId_AdmissionType_IsActive",
                table: "Prices");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Prices",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_SeasonId_AdmissionType_Channel_IsActive",
                table: "Prices",
                columns: new[] { "SeasonId", "AdmissionType", "Channel", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prices_SeasonId_AdmissionType_Channel_IsActive",
                table: "Prices");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Prices");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_SeasonId_AdmissionType_IsActive",
                table: "Prices",
                columns: new[] { "SeasonId", "AdmissionType", "IsActive" });
        }
    }
}
