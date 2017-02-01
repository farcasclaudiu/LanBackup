using System.Collections.Generic;
using LanBackup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System;

namespace LanBackup.WebApp.Controllers
{
  [Produces("application/json")]
  [Route("api/[controller]")]
  public class LanAgentsController : Controller
  {

    private const int MAXIMUM_ONLINE_DELAY = 15000;
    private static ConcurrentDictionary<string, StatusReportInfo> liveAgents = new ConcurrentDictionary<string, StatusReportInfo>();

    public bool UpdateAgentStatus(StatusReportInfo info)
    {
      liveAgents[info.IP] = info;
      return true;
    }



    /// <summary>
    /// retrieve all  backup configurations
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StatusReportInfo>), 200)]
    public async Task<IActionResult> Get()
    {
      return new OkObjectResult( liveAgents.Values.ToList());
    }

    //used internalyby SignalR
    public IEnumerable<StatusReportInfo> GetAgents() {
      return liveAgents.Values.ToList();
    }


    /// <summary>
    /// Refreshes online agents
    /// </summary>
    /// <returns>true if agents list updated</returns>
    public bool RefreshOnlineAgents() {
      var now = DateTime.UtcNow;
      List<string> toRemove = new List<string>();
      foreach (var item in liveAgents)
      {
        if (now > item.Value.StatusDateTime + TimeSpan.FromMilliseconds(MAXIMUM_ONLINE_DELAY))
          toRemove.Add(item.Key);
      }
      foreach (var keyRemove in toRemove)
      {
        StatusReportInfo removed;
        liveAgents.TryRemove(keyRemove, out removed);
      }
      return toRemove.Count > 0;
    }


  }
}
