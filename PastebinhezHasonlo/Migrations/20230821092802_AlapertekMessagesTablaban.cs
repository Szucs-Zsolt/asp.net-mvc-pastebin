using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastebinhezHasonlo.Migrations
{
    public partial class AlapertekMessagesTablaban : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "MessageId", "Msg" },
                values: new object[] { 1, "1", "Példaüzenet." });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "MessageId", "Msg" },
                values: new object[] { 2, "2", "Második üzenet." });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
