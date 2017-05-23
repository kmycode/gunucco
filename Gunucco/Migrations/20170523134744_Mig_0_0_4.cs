using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "SourceValue",
                table: "Media",
                nullable: false,
                defaultValue: (short)0)
                .Annotation("MySql:ValueGeneratedOnAdd", true);

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Chapter",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "PublicRangeValue",
                table: "Chapter",
                nullable: false,
                defaultValue: (short)101)
                .Annotation("MySql:ValueGeneratedOnAdd", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceValue",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "PublicRangeValue",
                table: "Chapter");
        }
    }
}
