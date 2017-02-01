using LanBackup.Models;
using LanBackupAgent.Message;
using LanBackupAgent.Utils;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using NLog;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TinyMessenger;

namespace LanBackupAgent.Controllers
{
  public class SignalRController : IDisposable
  {

    //TODO - move these in config file
    const string SIGNALR_HUBNAME = "backupslan";
    public const int SIGNALR_REFRESH_INTERVAL = 1500;
    public const int SIGNALR_REFRESH_INTERVAL_IDLE = 10000; //used for retry and ping (when idle)


    private ILogger logger;
    private ITinyMessengerHub messenger;
    private string webapiUrlBase;
    private Network networkUtil;
    private bool isWorkInProgress;

    #region SignalR Hub
    HubConnection connSignalR;
    private DateTime connSignalrClosedOn;
    IHubProxy hubSignalr;
    private System.Timers.Timer timerRetryPingSignalR = new System.Timers.Timer();
    #endregion


    public SignalRController(
        string mwebapiUrlBase,
        ILogger mlogger,
        ITinyMessengerHub mmessenger,
        Network networkUtil
      )
    {
      this.webapiUrlBase = mwebapiUrlBase;
      this.logger = mlogger;
      this.messenger = mmessenger;
      this.networkUtil = networkUtil;
      this.messenger.Subscribe<StopMessage>(DoStop);
      this.messenger.Subscribe<StatusReport>(DoStatus);
      logger.Trace("SignalRController instance initialized");
    }

    public async Task<bool> Start()
    {
      logger.Trace("SignalRController Starting");
      await ConfigureSignalR();
      ConfigureTimers();
      logger.Trace("SignalRController Started");
      return true;
    }

    private async Task ConfigureSignalR()
    {
      string webApiUrl = ConfigurationManager.AppSettings["webApiUrl"];
      string hubAddress = webApiUrl + "/signalr";
      logger.Trace($"Starting SignalR Client on: {hubAddress}");
      connSignalR = new HubConnection(hubAddress, useDefaultUrl: false);
      connSignalR.Error += (err) =>
      {
        if (err is WebSocketException)
        {
          return;
        }
        logger.Error($"Err: {err}");
      };
      connSignalR.Closed += () =>
      {
        logger.Info($"Connection closed.");
        connSignalrClosedOn = DateTime.Now;
      };
      connSignalR.ConnectionSlow += () =>
      {
        //TODO - log the issue
      };
      connSignalR.Reconnected += () =>
      {
        //TODO - check for overdue/due backups on the server
      };

      hubSignalr = connSignalR.CreateHubProxy(SIGNALR_HUBNAME);
      hubSignalr.On<string>("controlAgent", data =>  //Waiting control signal from server signalr
      {
        logger.Trace($"controlAgent data: {data}");
        //scheduler
        ThreadPool.QueueUserWorkItem((par) =>
        {
          try
          {
            DateTime recDate = (DateTime)par;
            //TODO - something to do by the server command
          }
          catch (Exception ex)
          {
            logger.Error($"ERR scheduler: {ex}");
          }
        }, DateTime.Now);
      });

      await TryStartSignalR();
    }




    #region Timer for retry

    private void ConfigureTimers()
    {
      //start retry timer for SignalR
      timerRetryPingSignalR.Interval = SIGNALR_REFRESH_INTERVAL_IDLE;
      timerRetryPingSignalR.Elapsed += TimerRetryPingSignalR_Elapsed;
      timerRetryPingSignalR.Start();

      logger.Trace("Timers configured");
    }


    private async void TimerRetryPingSignalR_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        if (connSignalR != null)
        {
          //retry
          if (connSignalR.State == ConnectionState.Disconnected)
          {
            if((DateTime.Now - connSignalrClosedOn).TotalSeconds > 5) //retry after 5 seconds
              await TryStartSignalR();
          }
          else if (connSignalR.State == ConnectionState.Connected)
          {
            //send ping signal when IDLE
            if(!isWorkInProgress)
            {
              // IDLE ping signalr - i'm online!
              ReadyIdlePing();
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error($"ERR TimerRetrySignalR: {ex}");
      }
    }


    /// <summary>
    /// signal thet agent is online
    /// </summary>
    private void ReadyIdlePing()
    {
      this.messenger.Publish(new StatusReport("", new StatusReportInfo()
      {
        IP = networkUtil.GetLocalIPAddress(),
        ConfigurationId = string.Empty,
        StatusType = StatusType.Idle,
        StatusDateTime = DateTime.UtcNow,
        StatusPercent = 0,
        StatusDescription = $"Ready ..."
      }));
    }

    public async Task TryStartSignalR()
    {
      try
      {
        logger.Info($"Trying ro connect ...");
        await connSignalR.Start();
        // hello to signalr - i'm online!
        ReadyIdlePing();
      }
      catch (HttpRequestException hex)
      {
        if (hex.InnerException is WebException)
        {
          WebException wex = hex.InnerException as WebException;
          if (wex.Status == WebExceptionStatus.ConnectFailure)
          {
            //ignore this err
          }
          else
          {
            logger.Error($"ERR WebException HResult: {wex.HResult}");
            logger.Error($"ERR WebException Status: {wex.Status}");
            logger.Error($"ERR WebException Message: {wex.Message}");
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error($"ERR Start SignalR: {ex}");
      }
    }

    #endregion Timer for retry



    #region SignalR actions
    private void DoStop(StopMessage obj)
    {
      //cleanup disposable resources
      if (timerRetryPingSignalR != null)
      {
        timerRetryPingSignalR.Stop();
        timerRetryPingSignalR.Dispose();
      }
      if (connSignalR != null)
      {
        connSignalR.Stop();
        connSignalR.Dispose();
      }

      logger.Trace("SignalRController Stopped");
    }


    private void DoStatus(StatusReport obj)
    {
      try
      {
        this.isWorkInProgress = (obj.Info.StatusType != StatusType.Idle);

        if (connSignalR != null)
        {
          logger.Warn($"DoStatus {JsonConvert.SerializeObject(obj)}");
          if (connSignalR != null && connSignalR.State == ConnectionState.Connected)
          {
            hubSignalr.Invoke("StatusReport", obj.Info);
          }
          else
          {
            //connection not open
          }
        }else
        {
          logger.Warn($"Warn SignalR DoStatus: connection null");
        }
      }
      catch (Exception ex)
      {
        logger.Error($"ERR SignalR DoStatus: {ex}");
      }
      
    }

    #endregion SignalR actions



    #region IDisposable

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (timerRetryPingSignalR != null)
        {
          timerRetryPingSignalR.Dispose();
        }
        if (connSignalR != null)
        {
          connSignalR.Dispose();
        }
      }
    }

    #endregion IDisposable

  }
}
