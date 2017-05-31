using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Gunucco.Models.Database;

namespace Gunucco.Migrations
{
    [DbContext(typeof(MainContext))]
    [Migration("20170530140541_Mig_0_0_8")]
    partial class Mig_0_0_8
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Gunucco.Entities.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

                    b.Property<string>("Name")
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Book");
                });

            modelBuilder.Entity("Gunucco.Entities.BookPermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("TargetId");

                    b.Property<short>("TargetTypeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)101);

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.ToTable("BookPermission");
                });

            modelBuilder.Entity("Gunucco.Entities.Chapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookId");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(128)");

                    b.Property<int>("Order");

                    b.Property<int?>("ParentId");

                    b.Property<short>("PublicRangeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)101);

                    b.HasKey("Id");

                    b.ToTable("Chapter");
                });

            modelBuilder.Entity("Gunucco.Entities.Content", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChapterId");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

                    b.Property<int>("Order");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.Property<short>("TypeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)101);

                    b.HasKey("Id");

                    b.ToTable("Content");
                });

            modelBuilder.Entity("Gunucco.Entities.Media", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContentId");

                    b.Property<short>("ExtensionValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)100);

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("varchar(256)");

                    b.Property<short>("SourceValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)101);

                    b.Property<short>("TypeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)101);

                    b.HasKey("Id");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("Gunucco.Entities.OauthCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("ExpireDateTime");

                    b.Property<short>("ScopeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue((short)0);

                    b.Property<string>("SessionId")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("UserTextId")
                        .HasColumnType("varchar(32)");

                    b.HasKey("Id");

                    b.ToTable("OauthCode");
                });

            modelBuilder.Entity("Gunucco.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("EmailHash")
                        .HasColumnType("varchar(128)");

                    b.Property<bool>("IsEmailValidated");

                    b.Property<string>("Name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<string>("TextId")
                        .IsRequired()
                        .HasColumnType("varchar(32)");

                    b.HasKey("Id");

                    b.HasIndex("TextId")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("Gunucco.Models.Entities.UserEmailValidation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("UserId");

                    b.Property<string>("ValidateKey")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.ToTable("UserEmailValidation");
                });

            modelBuilder.Entity("Gunucco.Models.Entities.UserSession", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("ExpireDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

                    b.Property<int>("ScopeValue")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("UserId");

                    b.Property<string>("UserTextId")
                        .IsRequired()
                        .HasColumnType("varchar(32)");

                    b.HasKey("Id");

                    b.ToTable("UserSession");
                });
        }
    }
}
