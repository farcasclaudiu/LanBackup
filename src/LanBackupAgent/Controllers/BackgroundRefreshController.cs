using Hangfire;
using Hangfire.SQLite;
using LanBackupAgent.WebApi;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Timers;
using TinyMessenger;
using LanBackupAgent.Models;
using LanBackupAgent.Message;

namespace LanBackupAgent.Utils
{
  public class BackgroundRefreshController : IDisposable
  {

    private ILogger logger;
    private ITinyMessengerHub messenger;
    private WebApiService webapi;

    public const int WEBAPI_REFRESH_BACKUPS_TIME = 120000; //miliseconds for backups refresh from webapi


    #region WebApi
    private System.Timers.Timer timerRefreshFromWebApi = new System.Timers.Timer();
    #endregion

    #region Hangfire
    private BackgroundJobServer hangfireServer;
    #endregion



    public BackgroundRefreshController(
      ILogger mlogger,
      ITinyMessengerHub mmessenger,
      WebApiService mwebapi
      )
    {
      this.logger = mlogger;
      this.messenger = mmessenger;
      this.webapi = mwebapi;

      this.messenger.Subscribe<StopMessage>(Stop);
      logger.Trace("BackgroundRefreshController instance initialized");
    }

    internal void Start()
    {
      ConfigureHangfire();
      ConfigureRefresWebApiTimer();
    }

    private void ConfigureHangfire()
    {
      string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "local_backups.db3");
      string sqlconn = $"Data Source={file};Version=3;";

      GlobalConfiguration.Configuration.UseSQLiteStorage(sqlconn);

      var options = new BackgroundJobServerOptions
      {
        // This is the default value
        SchedulePollingInterval = TimeSpan.FromSeconds(5),
        WorkerCount = 1// FORCE ONE BACKUP at one time - so they will be performed serial one by one
      };
      hangfireServer = new BackgroundJobServer(options);

      JobStorage.Current?.GetMonitoringApi()?.PurgeJobs();
      TimerRefreshFromWebApi_Elapsed(null, null);
    }

    private void ConfigureRefresWebApiTimer()
    {
      //start retry timer for WebAPi refresh
      timerRefreshFromWebApi = new System.Timers.Timer();
      timerRefreshFromWebApi.Interval = WEBAPI_REFRESH_BACKUPS_TIME;
      timerRefreshFromWebApi.Elapsed += TimerRefreshFromWebApi_Elapsed; ;
      timerRefreshFromWebApi.Start();

      logger.Trace("WebApi timer configured");
    }

    private async void TimerRefreshFromWebApi_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        logger.Trace(string.Format("WebApi refresh at {0}", DateTime.Now.ToString("o")));

        List<string> activeJobs = new List<string>();
        var result = await webapi.LoadBackupConfigurations();
        if (result != null)
        {
          foreach (var item in result)
          {
            try
            {
              string jobId = $"{item.Id}";// _{Convert.ToBase64String(item.RowVersion)}";
              if (item.IsActive.Value)
              {
                //RecurringJob.
                var recJob = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
                //RecurringJob.AddOrUpdate(jobId, () => LanCopyFileJob(item), item.Crontab);
                if (recJob == null)
                {
                  RecurringJob.AddOrUpdate(jobId, () => DoCopyJob(item), item.Crontab);
                }
                activeJobs.Add(jobId);
                RecurringJob.Trigger(jobId);
              }
              else
              {
                //stopping inactive
                var recJob = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
                if (recJob != null)
                {
                  RecurringJob.RemoveIfExists(jobId);
                }
              }
            }
            catch (Exception ex)
            {
              logger.Error($"Reccuring jobs refresh failed: {ex}");
              //report error to webAPI
              webapi.LogActivity(item.Id, $"Reccuring jobs refresh failed -({item.Id})- see Err", ex.Message, "ERR");
            }
          }
        }
        //TODO - purge ofphan jobs
        //JobStorage.Current.GetMonitoringApi().PurgeOrfanJobsExceptList(activeJobs);
      }
      catch (Exception ex)
      {
        logger.Error($"ERR WebApi refresh: {ex}");
        //report error to webAPI
        webapi.LogActivity(string.Empty, "Reccuring jobs refresh final failed - see Err", ex.Message, "ERR");
      }
    }


    //MUST BE STATIC so it can be called from Hangfire
    public static string DoCopyJob(BackupConfiguration item)
    {
      string result = string.Empty;
      try
      {
        result = DI.Container.GetInstance<LanCopyController>().StartLanCopyFileJob(item);
      }
      catch (Exception ex)
      {
        var logger = DI.Container.GetInstance<ILogger>();
        logger.Error(ex, "DoCopyJoberror:");
      }
      return result;
    }

    public void Stop(StopMessage msg)
    {
      //cleanup disposable resources
      if (timerRefreshFromWebApi != null)
      {
        timerRefreshFromWebApi.Stop();
        timerRefreshFromWebApi.Dispose();
      }
      if (hangfireServer != null)
      {
        hangfireServer.SendStop();
        hangfireServer.Dispose();
      }
      logger.Trace("LanCopyController Stopped");
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }



    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (timerRefreshFromWebApi != null)
        {
          timerRefreshFromWebApi.Dispose();
        }
        if (hangfireServer != null)
        {
          hangfireServer.Dispose();
        }
      }
    }
  }
}
