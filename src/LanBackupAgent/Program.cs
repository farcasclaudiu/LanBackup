using LanBackupAgent.Utils;
using LanBackupAgent.WebApi;
using LanBackupAgent.Controllers;
using NLog;
using SimpleInjector;
using System;
using System.Configuration;
using TinyMessenger;
using Topshelf;

namespace LanBackupAgent
{
  class Program
  {

    static ILogger logger;
    static TinyMessengerHub messenger;
    static Network network;

    public static void Main()
    {

      //conf logger
      logger = LogManager.GetCurrentClassLogger();
      messenger = new TinyMessengerHub();

      AppDomain currentDomain = AppDomain.CurrentDomain;
      currentDomain.UnhandledException += CurrentDomain_UnhandledException;


      //setup DI container
      // Register individual components
      DI.Container.Register<ILogger>(() => logger, Lifestyle.Singleton);
      DI.Container.Register<ITinyMessengerHub>(() => messenger, Lifestyle.Singleton);
      network = new Network(logger);
      DI.Container.Register<Network>(() => network, Lifestyle.Singleton);
      DI.Container.Register<BackgroundRefreshController>(Lifestyle.Transient);
      string webApiUrl = ConfigurationManager.AppSettings["webApiUrl"];
      WebApiService api = new WebApiService(webApiUrl, logger, messenger);
      DI.Container.Register<WebApiService>(() => api, Lifestyle.Singleton);
      SignalRController signarl = new SignalRController(webApiUrl, logger, messenger, network);
      DI.Container.Register<SignalRController>(() => signarl, Lifestyle.Singleton);
      DI.Container.Register<LanCopyController>(Lifestyle.Transient);
      logger.Trace("DI initialized");



      logger.Trace("Starting service");
      HostFactory.Run(x =>
      {
        //use NLog logger
        x.UseNLog();

        x.Service<LanBackupAgentService>(s =>
        {
          s.ConstructUsing(name => new LanBackupAgentService(logger, messenger));
          s.WhenStarted(async tc => await tc.Start());
          s.WhenStopped(tc => tc.Stop());
        });
        x.RunAsNetworkService();
        x.StartAutomatically(); // Start the service automatically

        x.SetDescription("LanBackup Agent Service");
        x.SetDisplayName("LanBackup Agent");
        //recommended that service names not contains spaces or other whitespace characters.
        x.SetServiceName("LanBackupAgent");


        x.EnableServiceRecovery(r =>
        {
          //you can have up to three of these
          //r.RestartComputer(5, "message");
          r.RestartService(1);//minutes for restart
          //the last one will act for all subsequent failures
          //r.RunProgram(7, "ping google.com");

          //should this be true for crashed or non-zero exits
          r.OnCrashOnly();

          //number of days until the error count resets
          r.SetResetPeriod(1);
        });


        //x.BeforeInstall(() => { ... });
        //x.AfterInstall(() => { ... });
        //x.BeforeUninstall(() => { ... });
        //x.AfterUninstall(() => { ... });

        x.OnException(ex =>
        {
          // Do something with the exception
          logger.Error(ex, "LanBackup Agent service Error: ");
        });

      });
      logger.Trace("Program DONE!");
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var logger = DI.Container.GetInstance<ILogger>();
      logger.Fatal($"Global exception: {e.ExceptionObject}");
    }

  }

}
