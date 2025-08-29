using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeverNeverLand.Data.Migrations
{
    public partial class PriceDataTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add new columns first
            migrationBuilder.AddColumn<string>(
                name: "Item",
                table: "Prices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Prices",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Ticket"); // sensible default

            // 2) Backfill data from AdmissionType -> Item, set Kind='Ticket'
            migrationBuilder.Sql(@"
                UPDATE P
                SET 
                    Kind = CASE WHEN ISNULL(Kind,'') = '' THEN 'Ticket' ELSE Kind END,
                    Item = CASE 
                               WHEN ISNULL(Item,'') = '' THEN ISNULL(AdmissionType,'') 
                               ELSE Item 
                           END
                FROM Prices AS P;
            ");

            // 3) Drop old index that references AdmissionType (if it exists)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes 
                          WHERE name = 'IX_Prices_SeasonId_AdmissionType_Channel_IsActive' 
                            AND object_id = OBJECT_ID('Prices'))
                BEGIN
                    DROP INDEX IX_Prices_SeasonId_AdmissionType_Channel_IsActive ON Prices;
                END
            ");

            // 4) Create the new composite index
            migrationBuilder.CreateIndex(
                name: "IX_Prices_SeasonId_Kind_Item_Channel_IsActive",
                table: "Prices",
                columns: new[] { "SeasonId", "Kind", "Item", "Channel", "IsActive" });

            // 5) Only now drop the legacy column
            migrationBuilder.DropColumn(
                name: "AdmissionType",
                table: "Prices");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Recreate old column
            migrationBuilder.AddColumn<string>(
                name: "AdmissionType",
                table: "Prices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // 2) Backfill AdmissionType from Item when Kind='Ticket'
            migrationBuilder.Sql(@"
                UPDATE P
                SET AdmissionType = CASE 
                    WHEN ISNULL(Item,'') <> '' AND Kind = 'Ticket' THEN Item 
                    ELSE ''
                END
                FROM Prices AS P;
            ");

            // 3) Drop new index
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes 
                          WHERE name = 'IX_Prices_SeasonId_Kind_Item_Channel_IsActive' 
                            AND object_id = OBJECT_ID('Prices'))
                BEGIN
                    DROP INDEX IX_Prices_SeasonId_Kind_Item_Channel_IsActive ON Prices;
                END
            ");

            // 4) Restore old index
            migrationBuilder.CreateIndex(
                name: "IX_Prices_SeasonId_AdmissionType_Channel_IsActive",
                table: "Prices",
                columns: new[] { "SeasonId", "AdmissionType", "Channel", "IsActive" });

            // 5) Optionally drop new columns (if you want a true rollback)
            migrationBuilder.DropColumn(name: "Item", table: "Prices");
            migrationBuilder.DropColumn(name: "Kind", table: "Prices");
        }
    }
}
