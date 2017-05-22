using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "TargetTypeValue",
                table: "BookPermission",
                nullable: false,
                defaultValue: (short)101,
                oldClrType: typeof(short))
                .Annotation("MySql:ValueGeneratedOnAdd", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "TargetTypeValue",
                table: "BookPermission",
                nullable: false,
                oldClrType: typeof(short),
                oldDefaultValue: (short)101)
                .OldAnnotation("MySql:ValueGeneratedOnAdd", true);
        }
    }
}
