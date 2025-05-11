using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedInitializationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
