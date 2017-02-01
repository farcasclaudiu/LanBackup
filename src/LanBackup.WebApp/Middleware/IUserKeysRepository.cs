using Microsoft.Extensions.Primitives;

namespace LanBackup.WebApp.Middleware
{
  public interface IUserKeysRepository
  {
    bool CheckValidUserKey(StringValues stringValues);
  }
}
