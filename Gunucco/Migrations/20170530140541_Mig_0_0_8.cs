using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OauthCode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Code = table.Column<string>(type: "varchar(64)", nullable: false),
                    ExpireDateTime = table.Column<DateTime>(nullable: false),
                    ScopeValue = table.Column<short>(nullable: false, defaultValue: (short)0)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    SessionId = table.Column<string>(type: "varchar(64)", nullable: true),
                    UserTextId = table.Column<string>(type: "varchar(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OauthCode", x => x.Id);
                });

            migrationBuilder.AlterColumn<string>(
                name: "ValidateKey",
                table: "UserEmailValidation",
                type: "varchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OauthCode");

            migrationBuilder.AlterColumn<string>(
                name: "ValidateKey",
                table: "UserEmailValidation",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)");
        }
    }
}
