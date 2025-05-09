using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumers_ConsumerGroups_ConsumerGroupId",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "KeyJson",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "KeyType",
                table: "Messages",
                newName: "Key");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ConsumerGroupMessageStatuses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Consumers_ConsumerGroups_ConsumerGroupId",
                table: "Consumers",
                column: "ConsumerGroupId",
                principalTable: "ConsumerGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumers_ConsumerGroups_ConsumerGroupId",
                table: "Consumers");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Messages",
                newName: "KeyType");

            migrationBuilder.AddColumn<string>(
                name: "KeyJson",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ConsumerGroupMessageStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumers_ConsumerGroups_ConsumerGroupId",
                table: "Consumers",
                column: "ConsumerGroupId",
                principalTable: "ConsumerGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
