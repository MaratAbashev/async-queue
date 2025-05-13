using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Messages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ConsumerPartition",
                columns: table => new
                {
                    ConsumersId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartitionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerPartition", x => new { x.ConsumersId, x.PartitionsId });
                    table.ForeignKey(
                        name: "FK_ConsumerPartition_Consumers_ConsumersId",
                        column: x => x.ConsumersId,
                        principalTable: "Consumers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumerPartition_Partitions_PartitionsId",
                        column: x => x.PartitionsId,
                        principalTable: "Partitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "IsDeleted", "TopicName" },
                values: new object[] { 1, false, "topic1" });

            migrationBuilder.InsertData(
                table: "ConsumerGroups",
                columns: new[] { "Id", "ConsumerGroupName", "IsDeleted", "TopicId" },
                values: new object[] { 1, "group1", false, 1 });

            migrationBuilder.InsertData(
                table: "Partitions",
                columns: new[] { "Id", "IsDeleted", "TopicId" },
                values: new object[,]
                {
                    { 1, false, 1 },
                    { 2, false, 1 }
                });

            migrationBuilder.InsertData(
                table: "ConsumerGroupOffsets",
                columns: new[] { "Id", "ConsumerGroupId", "IsDeleted", "PartitionId" },
                values: new object[,]
                {
                    { 1, 1, false, 1 },
                    { 2, 1, false, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPartition_PartitionsId",
                table: "ConsumerPartition",
                column: "PartitionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumerPartition");

            migrationBuilder.DeleteData(
                table: "ConsumerGroupOffsets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ConsumerGroupOffsets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ConsumerGroups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Partitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Partitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
