using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ExtensionValue",
                table: "Media",
                nullable: false,
                defaultValue: (short)100)
                .Annotation("MySql:ValueGeneratedOnAdd", true);

            migrationBuilder.AlterColumn<short>(
                name: "SourceValue",
                table: "Media",
                nullable: false,
                defaultValue: (short)101,
                oldClrType: typeof(short),
                oldDefaultValue: (short)0)
                .Annotation("MySql:ValueGeneratedOnAdd", true)
                .OldAnnotation("MySql:ValueGeneratedOnAdd", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtensionValue",
                table: "Media");

            migrationBuilder.AlterColumn<short>(
                name: "SourceValue",
                table: "Media",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldDefaultValue: (short)101)
                .Annotation("MySql:ValueGeneratedOnAdd", true)
                .OldAnnotation("MySql:ValueGeneratedOnAdd", true);
        }
    }
}
