using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LanBackup.DataCore.Migrations
{
  public class MigrationsContextFactory : IDbContextFactory<BackupsContext>
  {
    public BackupsContext Create(DbContextFactoryOptions foptions)
    {
      var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

      var options = new DbContextOptionsBuilder<BackupsContext>();
      options.UseSqlServer(config.GetConnectionString("BackupsConnectionString"));

      return new BackupsContext(options.Options);
    }
  }
}
