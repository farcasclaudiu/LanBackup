using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace LanBackup.WebApp.Middleware
{
  public class UserKeysRepository : IUserKeysRepository
  {

    List<string> _validKeys = new List<string>() {
      "12345" //TEST VALUE
    };
    public bool CheckValidUserKey(StringValues key)
    {
      return _validKeys.Contains(key);
    }
  }
}
