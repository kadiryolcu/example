using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                values: new object[] { new Guid("dfa80f44-4291-4f7c-ae33-a0c8d456dd47"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 11, 87, 202, 96, 14, 12, 8, 141, 197, 242, 190, 29, 166, 135, 10, 215, 226, 140, 243, 203, 35, 232, 98, 6, 51, 168, 79, 9, 201, 116, 72, 30, 224, 247, 40, 210, 21, 183, 174, 199, 173, 162, 128, 211, 248, 177, 242, 14, 156, 190, 73, 100, 74, 121, 198, 166, 39, 11, 82, 240, 232, 199, 108, 58 }, new byte[] { 50, 245, 230, 70, 128, 116, 224, 37, 217, 23, 121, 204, 159, 60, 79, 228, 177, 206, 16, 62, 49, 23, 169, 152, 140, 60, 187, 190, 134, 181, 174, 232, 22, 45, 87, 191, 195, 81, 227, 3, 175, 105, 254, 254, 127, 92, 49, 156, 229, 173, 215, 68, 244, 158, 50, 218, 228, 154, 57, 155, 199, 238, 212, 122, 6, 64, 189, 137, 97, 99, 166, 133, 67, 108, 232, 111, 111, 102, 136, 205, 170, 63, 21, 211, 24, 142, 234, 111, 150, 225, 247, 175, 138, 195, 75, 62, 234, 55, 129, 62, 55, 60, 130, 93, 11, 126, 242, 46, 69, 75, 227, 250, 176, 32, 18, 223, 26, 137, 117, 10, 207, 68, 63, 26, 190, 248, 165, 231 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("fed7d92a-4e8b-4bf4-a1a9-f8c63111799e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("dfa80f44-4291-4f7c-ae33-a0c8d456dd47") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("fed7d92a-4e8b-4bf4-a1a9-f8c63111799e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("dfa80f44-4291-4f7c-ae33-a0c8d456dd47"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("b83dc61b-ca28-42dd-86da-3b2b14026156"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 126, 84, 64, 126, 111, 114, 39, 208, 215, 234, 223, 197, 198, 168, 54, 189, 181, 74, 137, 157, 243, 195, 203, 1, 229, 145, 165, 64, 216, 51, 6, 49, 192, 15, 244, 30, 47, 204, 145, 55, 129, 94, 70, 224, 54, 176, 85, 94, 0, 106, 113, 245, 153, 101, 39, 3, 118, 109, 248, 134, 5, 137, 16, 197 }, new byte[] { 60, 131, 158, 215, 149, 122, 28, 54, 248, 130, 179, 77, 194, 196, 135, 38, 188, 42, 90, 199, 201, 154, 189, 243, 128, 243, 154, 54, 102, 140, 2, 52, 191, 74, 29, 25, 14, 56, 199, 48, 199, 255, 35, 228, 89, 195, 54, 250, 88, 115, 232, 0, 217, 52, 127, 42, 252, 135, 204, 69, 218, 180, 74, 212, 222, 76, 56, 69, 220, 38, 199, 24, 77, 30, 165, 66, 179, 191, 209, 8, 61, 243, 92, 121, 107, 19, 32, 44, 147, 210, 222, 101, 243, 181, 55, 203, 215, 239, 225, 105, 11, 135, 219, 190, 142, 60, 63, 85, 126, 150, 156, 87, 55, 124, 67, 93, 0, 199, 49, 202, 168, 233, 178, 222, 143, 245, 62, 108 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("f74a6677-99a7-4860-8253-a3d3c131d1a1"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("b83dc61b-ca28-42dd-86da-3b2b14026156") });
        }
    }
}
