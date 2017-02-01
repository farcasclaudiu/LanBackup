using LanBackup.DataCore;
using LanBackup.ModelsCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace LanBackup.WebApp.Tests.IntegrationTests
{
  public class MssqlTests : IDisposable
  {


    BackupsContext _context;

    public MssqlTests()
    {
      var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider();

      var builder = new DbContextOptionsBuilder<BackupsContext>();

      builder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=lanbackup_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true")
              .UseInternalServiceProvider(serviceProvider);

      _context = new BackupsContext(builder.Options);
      _context.Database.EnsureCreated();
      //_context.Database.Migrate();
    }
    public void Dispose()
    {
      _context.Database.EnsureDeleted();
    }






    #region MSSQL server integration tests

    [Trait("Category", "DbIntegration")]
    [Fact]
    public async void MssqlTestMethod()
    {

      //Add some monsters before querying
      _context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK"});
      _context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK"});
      _context.Add(new BackupLog { ClientIP = "192.168.1.101", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK"});
      _context.Add(new BackupLog { ClientIP = "192.168.1.102", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK"});
      _context.Add(new BackupLog { ClientIP = "192.168.1.103", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK"});
      _context.SaveChanges();
      //Execute the query
      var res = _context.Logs.FromSql("SELECT ID, Description, ClientIP, ConfigurationID, LogError, Status, DateTime, RowVersion FROM Logs").ToList();
      //Verify the results
      Assert.Equal(5, res.Count());

    }


    #endregion


  }
}
