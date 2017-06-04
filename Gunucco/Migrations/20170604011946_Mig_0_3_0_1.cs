using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_3_0_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Book",
                type: "varchar(1024)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Book",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("MySql:ValueGeneratedOnAdd", true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Book",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "PostToValue",
                table: "Book",
                nullable: false,
                defaultValue: (short)102)
                .Annotation("MySql:ValueGeneratedOnAdd", true);

            migrationBuilder.AlterColumn<int>(
                name: "ScopeValue",
                table: "OauthCode",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(short),
                oldDefaultValue: (short)0)
                .Annotation("MySql:ValueGeneratedOnAdd", true)
                .OldAnnotation("MySql:ValueGeneratedOnAdd", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Book");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Book");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Book");

            migrationBuilder.DropColumn(
                name: "PostToValue",
                table: "Book");

            migrationBuilder.AlterColumn<short>(
                name: "ScopeValue",
                table: "OauthCode",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(int),
                oldDefaultValue: 0)
                .Annotation("MySql:ValueGeneratedOnAdd", true)
                .OldAnnotation("MySql:ValueGeneratedOnAdd", true);
        }
    }
}
