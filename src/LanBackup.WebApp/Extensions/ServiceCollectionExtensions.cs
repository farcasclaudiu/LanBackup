using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LanBackup
{
  public static class ServiceCollectionExtensions
  {
    public static void AddEntityFramework<T>(this IServiceCollection services, string sqlConnectionString) where T : DbContext
    {
      services.AddDbContext<T>(options =>
              options.UseSqlServer(sqlConnectionString));
    }
  }
}
