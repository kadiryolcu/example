using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KeycloakIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                values: new object[] { new Guid("056a0be0-1e7b-4709-bd43-1ade4c53e1fd"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 218, 61, 92, 80, 101, 116, 149, 250, 84, 254, 197, 180, 140, 32, 47, 130, 120, 89, 128, 147, 200, 186, 147, 133, 174, 145, 26, 121, 130, 167, 157, 167, 244, 160, 44, 31, 88, 209, 149, 203, 131, 226, 6, 254, 118, 162, 221, 104, 129, 255, 61, 235, 105, 73, 62, 27, 47, 118, 129, 50, 244, 18, 230, 173 }, new byte[] { 115, 4, 209, 26, 215, 121, 196, 165, 135, 240, 175, 204, 115, 46, 214, 123, 104, 38, 160, 57, 207, 116, 161, 102, 167, 151, 138, 124, 151, 78, 209, 184, 44, 221, 57, 90, 120, 149, 123, 24, 46, 66, 131, 127, 222, 216, 41, 34, 227, 52, 81, 159, 55, 206, 243, 251, 226, 149, 93, 218, 66, 219, 115, 8, 100, 184, 131, 189, 44, 202, 177, 57, 146, 57, 127, 100, 191, 129, 219, 150, 243, 5, 17, 239, 149, 154, 210, 69, 178, 143, 112, 209, 6, 243, 65, 138, 154, 128, 50, 122, 81, 61, 212, 6, 91, 102, 133, 186, 106, 234, 221, 31, 241, 218, 169, 221, 134, 12, 46, 156, 88, 252, 129, 238, 67, 170, 147, 117 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("5fccfdc6-be29-4126-9afa-cf0b19882f57"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("056a0be0-1e7b-4709-bd43-1ade4c53e1fd") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserOperationClaims",
                keyColumn: "Id",
                keyValue: new Guid("5fccfdc6-be29-4126-9afa-cf0b19882f57"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("056a0be0-1e7b-4709-bd43-1ade4c53e1fd"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AuthenticatorType", "CreatedDate", "DeletedDate", "Email", "PasswordHash", "PasswordSalt", "UpdatedDate" },
                values: new object[] { new Guid("dfa80f44-4291-4f7c-ae33-a0c8d456dd47"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "narch@kodlama.io", new byte[] { 11, 87, 202, 96, 14, 12, 8, 141, 197, 242, 190, 29, 166, 135, 10, 215, 226, 140, 243, 203, 35, 232, 98, 6, 51, 168, 79, 9, 201, 116, 72, 30, 224, 247, 40, 210, 21, 183, 174, 199, 173, 162, 128, 211, 248, 177, 242, 14, 156, 190, 73, 100, 74, 121, 198, 166, 39, 11, 82, 240, 232, 199, 108, 58 }, new byte[] { 50, 245, 230, 70, 128, 116, 224, 37, 217, 23, 121, 204, 159, 60, 79, 228, 177, 206, 16, 62, 49, 23, 169, 152, 140, 60, 187, 190, 134, 181, 174, 232, 22, 45, 87, 191, 195, 81, 227, 3, 175, 105, 254, 254, 127, 92, 49, 156, 229, 173, 215, 68, 244, 158, 50, 218, 228, 154, 57, 155, 199, 238, 212, 122, 6, 64, 189, 137, 97, 99, 166, 133, 67, 108, 232, 111, 111, 102, 136, 205, 170, 63, 21, 211, 24, 142, 234, 111, 150, 225, 247, 175, 138, 195, 75, 62, 234, 55, 129, 62, 55, 60, 130, 93, 11, 126, 242, 46, 69, 75, 227, 250, 176, 32, 18, 223, 26, 137, 117, 10, 207, 68, 63, 26, 190, 248, 165, 231 }, null });

            migrationBuilder.InsertData(
                table: "UserOperationClaims",
                columns: new[] { "Id", "CreatedDate", "DeletedDate", "OperationClaimId", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("fed7d92a-4e8b-4bf4-a1a9-f8c63111799e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, new Guid("dfa80f44-4291-4f7c-ae33-a0c8d456dd47") });
        }
    }
}
