using LanBackupAgent.Controllers;
using LanBackupAgent.Message;
using LanBackupAgent.Utils;
using Microsoft.AspNet.SignalR.Client;
using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using TinyMessenger;

namespace LanBackupAgent
{
  public class LanBackupAgentService
  {

    private ILogger logger;
    ITinyMessengerHub messenger;


    public LanBackupAgentService(ILogger mlogger, ITinyMessengerHub mmessenger)
    {
      this.logger = mlogger;
      this.messenger = mmessenger;
    }

    public async Task<bool> Start()
    {
      logger.Info("Started");

      //START LOGIC
      DI.Container.GetInstance<BackgroundRefreshController>().Start();
      await DI.Container.GetInstance<SignalRController>().Start();

      return true;
    }




    public bool Stop()
    {
      //STOP LOGIC
      logger.Info("Stopping signal received ...");
      messenger.Publish(new StopMessage("STOP"));

      return true;
    }
  }
}
