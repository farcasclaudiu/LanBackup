using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using LanBackup.DataCore;
using LanBackup.ModelsCore;
using LanBackup.WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using LanBackup.WebApp.Models.Telemetry;
using LanBackup.WebApp.Models.DTO;

namespace LanBackup.WebApp.Tests.IntegrationTests
{
  public class ApiLogsControllerTests : IClassFixture<TestFixture<LanBackup.WebApp.Startup>>
  {

    private readonly HttpClient _client;
    private ITestOutputHelper _output;
    private ITelemetryLogger _tele;

    public ApiLogsControllerTests(TestFixture<LanBackup.WebApp.Startup> fixture, ITestOutputHelper output)
    {
      _output = output;
      _client = fixture.Client;

      Mapper.Initialize(cfg => cfg.AddProfiles(typeof(LanBackup.WebApp.Startup)));

      _tele = new MockTelemetry();

    }




    [Fact]
    public async Task GetAllLogRecords()
    {
      // Arrange

      // Act
      var response = await _client.GetAsync("/api/logs");

      // Assert
      response.EnsureSuccessStatusCode();
      var jsonStr = await response.Content.ReadAsStringAsync();
      var logList = JsonConvert.DeserializeObject<List<LanBackup.ModelsCore.BackupLog>>(jsonStr);
      
      Assert.NotEmpty(logList);
    }








    #region EF InMemory DB

    [Fact]
    public async void WithEfInMemoryTestMethod()
    {
      using (var mctx = new EfInMemoryContext().GetContext())
      {
        var service = new LogsController(Mapper.Instance, mctx, _tele);
        var res = Assert.IsType<OkObjectResult>(await service.Get(null, null));
        var val1 = Assert.IsType<List<BackupLogDTO>>(res.Value);

        Assert.Equal(5, val1.Count());
      }
    }

    private class EfInMemoryContext : IDisposable
    {
      private BackupsContext _context;
      private DbContextOptions<BackupsContext> options;

      public EfInMemoryContext()
      {
        options = new DbContextOptionsBuilder<BackupsContext>()
              .UseInMemoryDatabase()
              .Options;

        // Create the schema in the database
        using (var context = new BackupsContext(options))
        {
          context.Database.EnsureCreated();
        }

        // Insert seed data into the database using one instance of the context
        using (var context = new BackupsContext(options))
        {
          int id = 1;
          context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.101", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.102", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.103", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.SaveChanges();
        }
        _context = new BackupsContext(options);
      }

      public void Dispose()
      {
        _context.Dispose();
      }

      internal BackupsContext GetContext()
      {
        return _context;
      }
    }


    #endregion EF InMemory DB





    #region SqliteInMemory DB tests

    [Fact]
    public async void WithSqliteInmemoryTestMethod()
    {
      using (var mctx = new SqliteMemoryContext().GetContext())
      {
        var service = new LogsController(Mapper.Instance, mctx, _tele);
        var res = Assert.IsType<OkObjectResult>(await service.Get(null, null));
        var val1 = Assert.IsType<List<BackupLogDTO>>(res.Value);

        Assert.Equal(5, val1.Count());
      }
    }


    [Fact]
    public async void GetByIdWithSqliteInmemoryTestMethod()
    {

      using (var mctx = new SqliteMemoryContext().GetContext())
      {
        var recId = 1;
        var service = new LogsController(Mapper.Instance, mctx, _tele);

        var res = Assert.IsType<OkObjectResult>(service.Get(recId));
        var val1 = Assert.IsType<BackupLogDTO>(res.Value);

        Assert.NotNull(val1);

        var res2 = Assert.IsType<NotFoundResult>(service.Get(10000));
      }

    }


    [Fact]
    public async void GetByClientIdWithSqliteInmemoryTestMethod()
    {

      using (var mctx = new SqliteMemoryContext().GetContext())
      {
        var clientId = "192.168.1.100";
        var service = new LogsController(Mapper.Instance, mctx, _tele);

        var res = Assert.IsType<OkObjectResult>(service.GetByCientID(clientId));
        var val1 = Assert.IsType<List<BackupLogDTO>>(res.Value);

        Assert.Equal(2, val1.Count());
      }

    }




    private class SqliteMemoryContext : IDisposable
    {
      private SqliteConnection connection;
      private BackupsContext _context;
      private DbContextOptions<BackupsContext> options;

      public SqliteMemoryContext()
      {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        options = new DbContextOptionsBuilder<BackupsContext>()
              .UseSqlite(connection)
              .Options;

        // Create the schema in the database
        using (var context = new BackupsContext(options))
        {
          context.Database.EnsureCreated();
        }

        // Insert seed data into the database using one instance of the context
        using (var context = new BackupsContext(options))
        {
          int id = 1;
          context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.100", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.101", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.102", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.Add(new BackupLog { ClientIP = "192.168.1.103", Description = $"Playing with some test data {Guid.NewGuid()}", Status = "OK", ID = id++ });
          context.SaveChanges();
        }
        _context = new BackupsContext(options);
      }

      public void Dispose()
      {
        _context.Dispose();
        connection.Close();
        connection.Dispose();
      }

      internal BackupsContext GetContext()
      {
        return _context;
      }
    }


    #endregion



  }
}
