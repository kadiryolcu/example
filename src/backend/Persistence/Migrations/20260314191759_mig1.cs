using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("ac49eb3f-8e10-4c90-8f53-7d07c179f27e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e98bf2d8-fa91-41ee-905f-35b2ecb88fc1"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("b83dc61b-ca28-42dd-86da-3b2b14026156"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 126, 84, 64, 126, 111, 114, 39, 208, 215, 234, 223, 197, 198, 168, 54, 189, 181, 74, 137, 157, 243, 195, 203, 1, 229, 145, 165, 64, 216, 51, 6, 49, 192, 15, 244, 30, 47, 204, 145, 55, 129, 94, 70, 224, 54, 176, 85, 94, 0, 106, 113, 245, 153, 101, 39, 3, 118, 109, 248, 134, 5, 137, 16, 197 }, new byte[] { 60, 131, 158, 215, 149, 122, 28, 54, 248, 130, 179, 77, 194, 196, 135, 38, 188, 42, 90, 199, 201, 154, 189, 243, 128, 243, 154, 54, 102, 140, 2, 52, 191, 74, 29, 25, 14, 56, 199, 48, 199, 255, 35, 228, 89, 195, 54, 250, 88, 115, 232, 0, 217, 52, 127, 42, 252, 135, 204, 69, 218, 180, 74, 212, 222, 76, 56, 69, 220, 38, 199, 24, 77, 30, 165, 66, 179, 191, 209, 8, 61, 243, 92, 121, 107, 19, 32, 44, 147, 210, 222, 101, 243, 181, 55, 203, 215, 239, 225, 105, 11, 135, 219, 190, 142, 60, 63, 85, 126, 150, 156, 87, 55, 124, 67, 93, 0, 199, 49, 202, 168, 233, 178, 222, 143, 245, 62, 108 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("f74a6677-99a7-4860-8253-a3d3c131d1a1"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("b83dc61b-ca28-42dd-86da-3b2b14026156") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("f74a6677-99a7-4860-8253-a3d3c131d1a1"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b83dc61b-ca28-42dd-86da-3b2b14026156"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("e98bf2d8-fa91-41ee-905f-35b2ecb88fc1"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 109, 237, 217, 247, 48, 21, 8, 150, 218, 142, 76, 63, 177, 247, 81, 149, 63, 138, 203, 81, 131, 44, 33, 252, 222, 50, 69, 36, 217, 25, 166, 231, 217, 174, 116, 135, 228, 211, 69, 246, 202, 107, 150, 232, 112, 22, 209, 49, 12, 100, 52, 207, 86, 83, 87, 104, 160, 72, 194, 9, 149, 243, 238, 52 }, new byte[] { 116, 211, 164, 17, 35, 206, 200, 156, 221, 156, 58, 216, 98, 211, 160, 102, 156, 122, 193, 68, 196, 188, 126, 254, 198, 208, 37, 53, 112, 179, 197, 138, 4, 11, 203, 69, 141, 189, 30, 98, 205, 21, 59, 203, 50, 120, 72, 214, 44, 214, 213, 125, 84, 112, 196, 22, 8, 11, 52, 6, 65, 208, 224, 176, 130, 232, 100, 252, 239, 141, 32, 135, 126, 118, 192, 184, 153, 20, 40, 56, 0, 84, 179, 86, 27, 182, 244, 179, 58, 235, 243, 87, 216, 200, 185, 92, 109, 113, 77, 159, 161, 235, 71, 100, 117, 189, 198, 86, 119, 63, 87, 91, 205, 83, 11, 255, 195, 154, 75, 164, 228, 206, 110, 184, 35, 208, 67, 177 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("ac49eb3f-8e10-4c90-8f53-7d07c179f27e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("e98bf2d8-fa91-41ee-905f-35b2ecb88fc1") });
        }
    }
}
