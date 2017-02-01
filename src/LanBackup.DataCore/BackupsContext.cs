using LanBackup.ModelsCore;
using Microsoft.EntityFrameworkCore;

namespace LanBackup.DataCore
{
  public class BackupsContext : DbContext
  {
    public BackupsContext(DbContextOptions<BackupsContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      //BackupConfiguration - entity
      modelBuilder.Entity<BackupConfiguration>().ToTable("Backups");
      modelBuilder.Entity<BackupConfiguration>().Property(p => p.RowVersion)
        .IsRowVersion()
        .IsConcurrencyToken();
      modelBuilder.Entity<BackupConfiguration>().HasIndex(p => p.ClientIP);

      //BackupLog - entity
      modelBuilder.Entity<BackupLog>().Property(p => p.ID).ValueGeneratedOnAdd(); //equivalent of [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      modelBuilder.Entity<BackupLog>().HasIndex(p => p.ClientIP);
      modelBuilder.Entity<BackupLog>().HasIndex(p => p.ConfigurationID);

    }

    public DbSet<BackupConfiguration> Backups { get; set; }
    public DbSet<BackupLog> Logs { get; set; }
  }
}
