using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEmailValidation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    UserId = table.Column<int>(nullable: false),
                    ValidateKey = table.Column<string>(type: "varchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmailValidation", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "EmailHash",
                table: "User",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailValidated",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailHash",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsEmailValidated",
                table: "User");

            migrationBuilder.DropTable(
                name: "UserEmailValidation");
        }
    }
}
