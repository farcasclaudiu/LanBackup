using System.Net.Http;
using Xunit;

namespace LanBackup.WebApp.Tests.IntegrationTests
{
  public class HomeControllerTests : IClassFixture<TestFixture<LanBackup.WebApp.Startup>>
  {
    private readonly HttpClient _client;

    public HomeControllerTests(TestFixture<LanBackup.WebApp.Startup> fixture)
    {
      _client = fixture.Client;
    }

  }
}
