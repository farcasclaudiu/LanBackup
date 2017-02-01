using LanBackup.Models;
using LanBackup.WebApp.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanBackup.Hubs
{
  [HubName("backupslan")]
  public class LanBackupsHub : Hub
  {

    private LanAgentsController _agents;
    private Timer _cleanTimer;

    public LanBackupsHub(
      LanAgentsController agents
      )
    {
      this._agents = agents;
      _cleanTimer = new Timer(CheckOnlineAgents, null, TimeSpan.FromMilliseconds(5000), TimeSpan.FromMilliseconds(5000));
    }

    private void CheckOnlineAgents(object state)
    {
      if(this._agents.RefreshOnlineAgents())
      {
        Clients.All.agentsRefresh(_agents.GetAgents());
      }
    }


    //calls from agents to server
    public bool StatusReport(StatusReportInfo info)
    {
       _agents.UpdateAgentStatus(info);
       Clients.All.agentsRefresh(_agents.GetAgents());
       return true;
    }


    //calls from server to agents
    public void controlAgent(string agentId, string message) {  //TODO - message can be configID, TEST_SOURCE or TEST DEST
      Clients.Client(agentId).controlAgent(message);
    }



    
    
  }

}
