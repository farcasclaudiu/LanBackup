using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LanBackup.WebApp.Middleware
{
  public class UserKeyValidatorsMiddleware
  {
    private readonly RequestDelegate _next;
    private IUserKeysRepository _keysRepo { get; set; }

    public UserKeyValidatorsMiddleware(RequestDelegate next, IUserKeysRepository _repo)
    {
      _next = next;
      _keysRepo = _repo;
    }

    public async Task Invoke(HttpContext context)
    {
      if (!context.User.Identity.IsAuthenticated)
      {

        if (context.Request.Headers.Keys.Contains("user-key") &&
          _keysRepo.CheckValidUserKey(context.Request.Headers["user-key"]))
        {

          string userName = "LanBackupAgent";
          var usernameClaim = new Claim(ClaimTypes.Name, userName);
          var identity = new ClaimsIdentity(new[] { usernameClaim }, "ApiKey");
          var principal = new ClaimsPrincipal(identity);
          AuthenticationProperties prop = new AuthenticationProperties()
          {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddMilliseconds(1000)
          };
          await context.Authentication.SignInAsync(
            Startup.AuthenticationSchemeName
            //"KeyValueMiddleware"
            , principal);
        }
        else
        {
          if (context.Request.Path.ToString().Contains(@"/api/"))
          {
            context.Response.StatusCode = 401; //UnAuthorized
            await context.Response.WriteAsync("{ err: 'unauthorized' }");
            return;
          }
        }


        //if (!context.Request.Headers.Keys.Contains("user-key"))
        //{
        //  context.Response.StatusCode = 400; //Bad Request                
        //  await context.Response.WriteAsync("User Key is missing");
        //  return;
        //}
        //else
        //{
        //  if (!_keysRepo.CheckValidUserKey(context.Request.Headers["user-key"]))
        //  {
        //    context.Response.StatusCode = 401; //UnAuthorized
        //    await context.Response.WriteAsync("Invalid User Key");
        //    return;
        //  }
        //}

      }

      await _next.Invoke(context);
    }

  }

  #region ExtensionMethod
  public static class UserKeyValidatorsExtension
  {
    public static IApplicationBuilder ApplyUserKeyValidation(this IApplicationBuilder app)
    {
      app.UseMiddleware<UserKeyValidatorsMiddleware>();
      return app;
    }
  }
  #endregion
}
