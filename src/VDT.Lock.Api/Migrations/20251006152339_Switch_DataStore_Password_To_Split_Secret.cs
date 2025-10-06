using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VDT.Lock.Api.Migrations
{
    /// <inheritdoc />
    public partial class Switch_DataStore_Password_To_Split_Secret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "DataStores");

            migrationBuilder.AddColumn<byte[]>(
                name: "SecretHash",
                table: "DataStores",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "SecretSalt",
                table: "DataStores",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretHash",
                table: "DataStores");

            migrationBuilder.DropColumn(
                name: "SecretSalt",
                table: "DataStores");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "DataStores",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
