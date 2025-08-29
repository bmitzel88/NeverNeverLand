using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeverNeverLand.Data.Migrations
{
    /// <inheritdoc />
    public partial class PriceLogFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ParkPass");

            migrationBuilder.RenameColumn(
                name: "ValidUntil",
                table: "ParkPass",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ParkPass",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ParkPass",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "AdmissionType",
                table: "Ticket",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HolderAge",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HolderName",
                table: "Ticket",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePaid",
                table: "Ticket",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            // 🔽 Fix: drop and recreate ParkPass.Id as GUID instead of AlterColumn
            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkPass",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ParkPass");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ParkPass",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkPass",
                table: "ParkPass",
                column: "Id");

            migrationBuilder.AddColumn<int>(
                name: "MaxGuests",
                table: "ParkPass",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxOwners",
                table: "ParkPass",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "ParkPass",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeasonYear",
                table: "ParkPass",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PriceChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: true),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NewAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    AlwaysOn = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    AdmissionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EffectiveStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prices_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prices_SeasonId_AdmissionType_IsActive",
                table: "Prices",
                columns: new[] { "SeasonId", "AdmissionType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_IsActive_AlwaysOn_StartDate_EndDate",
                table: "Seasons",
                columns: new[] { "IsActive", "AlwaysOn", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceChangeLogs");

            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropColumn(
                name: "AdmissionType",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "HolderAge",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "HolderName",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "PricePaid",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaxGuests",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "MaxOwners",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "SeasonYear",
                table: "ParkPass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkPass",
                table: "ParkPass");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ParkPass");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ParkPass",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkPass",
                table: "ParkPass",
                column: "Id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ParkPass",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ParkPass",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "ParkPass",
                newName: "ValidUntil");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ParkPass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ParkPass",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ParkPass",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
