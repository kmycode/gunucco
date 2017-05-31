using Gunucco.Entities;
using Gunucco.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Database
{
    public class MainContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public DbSet<User> User { get; set; }

        public DbSet<UserEmailValidation> UserEmailValidation { get; set; }

        public DbSet<UserSession> UserSession { get; set; }

        public DbSet<OauthCode> OauthCode { get; set; }

        public DbSet<Book> Book { get; set; }

        public DbSet<Chapter> Chapter { get; set; }

        public DbSet<Content> Content { get; set; }

        public DbSet<Media> Media { get; set; }

        public DbSet<BookPermission> BookPermission { get; set; }

        public MainContext() : base(new DbContextOptionsBuilder().UseMySql(ConnectionString).Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.TextId)
                    .IsRequired()
                    .HasColumnType("varchar(32)");
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasColumnType("varchar(128)");
                entity.Property(e => e.Description)
                    .HasColumnType("varchar(1024)");
                entity.Property(e => e.EmailHash)
                    .HasColumnType("varchar(128)");
            });
            builder.Entity<User>().HasIndex(u => u.TextId).IsUnique();

            builder.Entity<UserEmailValidation>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.ValidateKey)
                    .IsRequired()
                    .HasColumnType("varchar(128)");
            });

            builder.Entity<UserSession>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("varchar(64)");
                entity.Property(e => e.UserId)
                    .IsRequired();
                entity.Property(e => e.UserTextId)
                    .IsRequired()
                    .HasColumnType("varchar(32)");
                entity.Property(e => e.ExpireDateTime)
                    .IsRequired()
                    .HasDefaultValue(DateTime.MinValue);
                entity.Property(e => e.ScopeValue)
                    .IsRequired()
                    .HasDefaultValue((int)Scope.None);
            });

            builder.Entity<OauthCode>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(64)");
                entity.Property(e => e.ScopeValue)
                    .IsRequired()
                    .HasDefaultValue((short)Scope.None);
                entity.Property(e => e.SessionId)
                    .HasColumnType("varchar(64)");
                entity.Property(e => e.UserTextId)
                    .HasColumnType("varchar(32)");
                entity.Property(e => e.UserId);
            });

            builder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired();
                entity.Property(e => e.Name)
                    .HasColumnType("varchar(128)");
                entity.Property(e => e.Created)
                    .IsRequired()
                    .HasDefaultValue(DateTime.MinValue);
            });

            builder.Entity<Chapter>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired();
                entity.Property(e => e.BookId)
                    .IsRequired();
                entity.Property(e => e.Name)
                    .HasColumnType("varchar(128)");
                entity.Property(e => e.PublicRangeValue)
                    .IsRequired()
                    .HasDefaultValue((short)PublishRange.All);
                entity.Property(e => e.Order)
                    .IsRequired();
            });

            builder.Entity<Content>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired();
                entity.Property(e => e.TypeValue)
                    .IsRequired()
                    .HasDefaultValue((short)ContentType.Text);
                entity.Property(e => e.Text)
                    .HasColumnType("text");
                entity.Property(e => e.Order)
                    .IsRequired();
                entity.Property(e => e.LastModified)
                    .HasDefaultValue(DateTime.MinValue);
                entity.Property(e => e.Created)
                    .IsRequired()
                    .HasDefaultValue(DateTime.MinValue);
            });

            builder.Entity<Media>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired();
                entity.Property(e => e.TypeValue)
                    .IsRequired()
                    .HasDefaultValue((short)MediaType.Image);
                entity.Property(e => e.FilePath)
                    .IsRequired()
                    .HasColumnType("varchar(256)");
                entity.Property(e => e.ContentId)
                    .IsRequired();
                entity.Property(e => e.SourceValue)
                    .IsRequired()
                    .HasDefaultValue((short)MediaSource.Self);
                entity.Property(e => e.ExtensionValue)
                    .IsRequired()
                    .HasDefaultValue((short)MediaExtension.Outside);
            });

            builder.Entity<BookPermission>(entity =>
            {
                entity.Property(e => e.Id)
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .IsRequired();
                entity.Property(e => e.TargetTypeValue)
                    .IsRequired()
                    .HasDefaultValue((short)TargetType.Book);
            });
        }
    }
}
