using LanBackupAgent.Message;
using LanBackupAgent.Models;
using LanBackupAgent.Utils;
using Microsoft.Rest;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TinyMessenger;

namespace LanBackupAgent.WebApi
{
  public class WebApiService
  {
    private ILogger logger;
    private ITinyMessengerHub messenger;
    private string webapiUrlBase;

    public WebApiService(string mwebapiUrlBase,
       ILogger mlogger,
      ITinyMessengerHub mmessenger
      )
    {
      this.webapiUrlBase = mwebapiUrlBase;
      this.logger = mlogger;
      this.messenger = mmessenger;

      this.messenger.Subscribe<StopMessage>(Stop);
      logger.Trace("WebApiService instance initialized");
    }

    private void Stop(StopMessage obj)
    {
      //TODO - implement cancelation
    }

    public async Task<IList<BackupConfiguration>> LoadBackupConfigurations()
    {
      IList<BackupConfiguration> result = new List<BackupConfiguration>();
      try
      {
        LanBackupsAPI api = getApi();
        //retrieve own backup configurations
        string localIp = DI.Container.GetInstance<Network>().GetLocalIPAddress();
        result = await api.ApiBackupConfigClientByClientidGetAsync(localIp);

        if (result != null)
        {
          logger.Info($"loadBackupConfigurations WebApi, response: {result.Count} configurations.");
        }
        else
        {
          logger.Info($"loadBackupConfigurations WebApi, response: NO configurations.");
        }
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
        logger.Error($"loadBackupConfigurations ERROR: {ex}");
      }
      return result;
    }



    public async void LogActivity(string backupId, string message, string error, string status)
    {
      try
      {
        LanBackupsAPI api = getApi();
        // submit activity status
        var log = new BackupLog()
        {
          ClientIP = DI.Container.GetInstance<Network>().GetLocalIPAddress(),
          Status = status,
          Description = message,
          LogError = error,
          DateTime = DateTime.UtcNow
        };
        var result = await api.ApiLogsPostAsync(log);

        logger.Info($"Log sent to WebApi, response ID: {result}");
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
        logger.Error($"WebApi ERROR: {ex}");
      }

    }



    #region helper

    private LanBackupsAPI getApi()
    {
      string webApiUrl = webapiUrlBase;
      var credentials = new TokenCredentials("<bearer token>");
      LanBackupsAPI api = new LanBackupsAPI(new Uri(
        webApiUrl
        ), credentials);
      return api;
    }

    #endregion helper

  }
}
