using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LanBackup.DataCore;

namespace LanBackup.DataCore.Migrations
{
    [DbContext(typeof(BackupsContext))]
    partial class BackupsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LanBackup.ModelsCore.BackupConfiguration", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClientIP")
                        .IsRequired();

                    b.Property<string>("Crontab");

                    b.Property<string>("DestLanFolder");

                    b.Property<string>("DestPass");

                    b.Property<string>("DestUser");

                    b.Property<bool>("IsActive");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("SrcFolder");

                    b.Property<string>("SrcPass");

                    b.Property<string>("SrcUser");

                    b.HasKey("ID");

                    b.HasIndex("ClientIP");

                    b.ToTable("Backups");
                });

            modelBuilder.Entity("LanBackup.ModelsCore.BackupLog", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClientIP");

                    b.Property<string>("ConfigurationID");

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Description");

                    b.Property<string>("LogError");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Status");

                    b.HasKey("ID");

                    b.HasIndex("ClientIP");

                    b.HasIndex("ConfigurationID");

                    b.ToTable("Logs");
                });
        }
    }
}
