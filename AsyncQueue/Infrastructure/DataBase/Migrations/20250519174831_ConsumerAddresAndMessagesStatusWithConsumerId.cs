using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class ConsumerAddresAndMessagesStatusWithConsumerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Consumers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConsumerId",
                table: "ConsumerGroupMessageStatuses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerGroupMessageStatuses_ConsumerId",
                table: "ConsumerGroupMessageStatuses",
                column: "ConsumerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumerGroupMessageStatuses_Consumers_ConsumerId",
                table: "ConsumerGroupMessageStatuses",
                column: "ConsumerId",
                principalTable: "Consumers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumerGroupMessageStatuses_Consumers_ConsumerId",
                table: "ConsumerGroupMessageStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ConsumerGroupMessageStatuses_ConsumerId",
                table: "ConsumerGroupMessageStatuses");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Consumers");

            migrationBuilder.DropColumn(
                name: "ConsumerId",
                table: "ConsumerGroupMessageStatuses");
        }
    }
}
