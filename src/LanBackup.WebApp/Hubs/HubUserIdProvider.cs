using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace LanBackup.WebApp.Hubs
{
  internal class HubUserIdProvider : IUserIdProvider
  {
    public string GetUserId(HttpRequest request)
    {
      string ip = request.Query["ip"];
      //request.HttpContext.User.Identity.Name;
      return ip;
    }
  }
}