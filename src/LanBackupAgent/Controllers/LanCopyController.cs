using System;
using LanBackupAgent.Models;
using NLog;
using LanBackupAgent.WebApi;
using TinyMessenger;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Security.Principal;
using SimpleImpersonation;
using LanBackupAgent.Message;
using LanBackup.Models;
using System.Linq;
using LanBackupAgent.Controllers;

namespace LanBackupAgent.Utils
{
  public class LanCopyController
  {

    private ILogger logger;
    private ITinyMessengerHub messenger;
    private WebApiService webapi;
    private Exception externalError;
    private Network networkUtil;


    /// <summary>
    /// used to signal a forced exception
    /// </summary>
    private bool forceExceptionalStop;


    /// <summary>
    /// used to queue and sync copy jobs between threads
    /// </summary>
    ConcurrentQueue<CopyOperation> myCollection;

    /// <summary>
    /// maximum items in the copy queue - must test to find the best value
    /// </summary>
    const int queueTHRESHOLD = 30;

    /// <summary>
    /// buffer size for copy operations - must test to find the best value
    /// </summary>
    const int bufferSize = 5*65536;



    private long dirCount = 0; long fileCount = 0;



    public LanCopyController(
      ILogger mlogger,
      ITinyMessengerHub mmessenger,
      WebApiService mwebapi,
      Network networkUtil
      )
    {
      this.logger = mlogger;
      this.messenger = mmessenger;
      this.webapi = mwebapi;
      this.networkUtil = networkUtil;

      this.messenger.Subscribe<StopMessage>(Stop);
      logger.Trace("LanCopyController instance initialized");
    }

    public string StartLanCopyFileJob(BackupConfiguration item)
    {
      string jobId = $"{item.Id}";// _{Convert.ToBase64String(item.RowVersion)}";
      Exception localEx = null;
      try
      {
        var msg = $"({item.Id}) - START";
        logger.Debug(msg);
        webapi.LogActivity(item.Id, msg, string.Empty, "OK");
        ReportAgentStatus(StatusType.Starting, $"Starting copy operation - ({item.Id})", 0, item.Id);

        if (item.SrcUser.IndexOf(@"@") < 0)
          throw new ArgumentException("Source user must be in format john@computer");
        if (item.DestUser.IndexOf(@"@") < 0)
          throw new ArgumentException("Destination user must be in format john@computer");


        string domainSrc = item.SrcUser.Substring(item.SrcUser.IndexOf(@"@") + 1);
        string usernameSrc = item.SrcUser.Substring(0, item.SrcUser.IndexOf(@"@"));
        string domainDest = item.DestUser.Substring(item.DestUser.IndexOf(@"@") + 1);
        string usernameDest = item.DestUser.Substring(0, item.DestUser.IndexOf(@"@"));

        Exception outEx;
        if (!CopyFolderAndFilesWithImpersonation(item.Id, item.SrcFolder, domainSrc, usernameSrc, item.SrcPass,
          item.DestLanFolder, domainDest, usernameDest, item.DestPass, out outEx))
        {
          if (outEx != null)
          {
            localEx = outEx;
            logger.Error($"CopyFolderAndFilesWithImpersonation ERR : {outEx}");
            ReportAgentStatus(StatusType.Error, "Error... " + outEx.Message, 100);
          }
        }
        else
        {
          ReportAgentStatus(StatusType.Idle, $"Finished - ({item.Id})...", 100);
        }
      }
      catch (Exception ex)
      {
        localEx = ex;
        logger.Error($"Background job ERR : {ex}");
        ReportAgentStatus(StatusType.Error, $"Error - ({item.Id}) - " + ex.Message, 0);
      }
      finally
      {
        var msg = $"({item.Id}) - END";
        logger.Debug(msg);
        webapi.LogActivity(item.Id, msg, localEx != null ? localEx.Message : string.Empty, localEx != null ? "ERR" : "OK");
      }


      return jobId;
    }

    private void ReportAgentStatus(StatusType type, string desc, int percent, string configId = "")
    {
      var obj = new StatusReport("", new StatusReportInfo()
      {
        IP = networkUtil.GetLocalIPAddress(),
        ConfigurationId = configId,
        StatusType = type,
        StatusDateTime = DateTime.UtcNow,
        StatusPercent = percent,
        StatusDescription = desc
      });
      ThreadPool.QueueUserWorkItem((msg) => {
        this.messenger.Publish(msg as StatusReport);
      }, obj);
    }






    /// <summary>
    /// Copy the source folder content into a destination folder
    /// </summary>
    /// <param name="srcPath">source folder</param>
    /// <param name="destPath">destination folder</param>
    /// <returns>true if success</returns>
    private bool CopyFolderAndFilesWithImpersonation(string configID, string srcPath, string domainSrc, string userSrc, string passSrc,
      string destPath, string domainDest, string userDest, string passDest, out Exception outEx)
    {
      outEx = null;
      myCollection = new ConcurrentQueue<CopyOperation>();

      byte[] buffer = new byte[bufferSize];
      bool finished = false;
      Exception srcError = null;
      Exception destError = null;
      Thread thSource = new Thread(() =>
      {
        try
        {
          //FileLogging.Log($"begin thSrc.", LogLevel.Info);
          using (Impersonation.LogonUser(domainSrc, userSrc, passSrc, LogonType.Network))//LogonType.Interactive
          {
            WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
            //FileLogging.Log($"impersonated thSrc.  {wid_current.Name}", LogLevel.Info);
            //scan all of the directories
            var lstDirectories = Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories);
            dirCount = lstDirectories.LongCount();
            foreach (string dirPath in lstDirectories)
            {
              CopyOperation coFolder = new CopyOperation
              {
                Operation = OperationType.WriteFolder,
                Argument = dirPath.Replace(srcPath, string.Empty)
              };
              FillQueue(myCollection, queueTHRESHOLD, destError, coFolder);
              if (forceExceptionalStop)
                return;
              //FileLogging.Log($"q-d: {coFolder.Argument}", LogLevel.Info);
            }
            //scan all files
            var lstFiles = Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories);
            fileCount = lstFiles.LongCount();
            foreach (string newPath in lstFiles)
            {
            using (FileStream fs = File.Open(newPath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.Read))
              {
                int readB;
                long offset = 0;
                while ((readB = fs.Read(buffer, 0, bufferSize)) > 0)
                {
                  CopyOperation coFile = new CopyOperation
                  {
                    Operation = OperationType.WriteFile,
                    Argument = newPath.Replace(srcPath, string.Empty),
                    Offset = offset,
                    Content = buffer.SubArray(0, readB)
                  };
                  FillQueue(myCollection, queueTHRESHOLD, destError, coFile);
                  if (forceExceptionalStop)
                    return;
                  //FileLogging.Log($"q-f: {coFile.Argument} - {coFile.Offset}", LogLevel.Info);
                  offset += readB;
                }
              }
            }
            //YEY!
            finished = true;

            //wic.Undo();
          }
        }
        catch (Exception err)
        {
          srcError = err;
          forceExceptionalStop = true;
          finished = true;
        }
      });


      Thread thDest = new Thread(() =>
      {
        try
        {
          //FileLogging.Log($"begin thDest.", LogLevel.Info);
          Thread.Sleep(200);

          long dirIndex = 0, fileIndex = 0;
          string lastFile = string.Empty;
          DateTime lastSent = DateTime.MinValue;
          int copyPercent;
          using (Impersonation.LogonUser(domainDest, userDest, passDest, LogonType.NewCredentials))//LogonType.NewCredentials
          {
            WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
            //FileLogging.Log($"impersonated thDest.  {wid_current.Name}", LogLevel.Info);
            while (!finished || myCollection.Count > 0)
            {
              while (myCollection.Count == 0 && !forceExceptionalStop)
              {
                Thread.Sleep(100);
              }
              if (forceExceptionalStop)
                return;

              CopyOperation item = null;
              if (myCollection.TryDequeue(out item))
              {
                switch (item.Operation)
                {
                  case OperationType.WriteFolder:
                    string newFolder = UncCombine(destPath, item.Argument);
                    Directory.CreateDirectory(newFolder);
                    dirIndex++;
                    break;
                  case OperationType.WriteFile:
                    string destFile = UncCombine(destPath, item.Argument);
                    using (var fs = ((item.Offset == 0) && File.Exists(destFile)) ? File.Create(destFile) : File.OpenWrite(destFile))
                    {
                      fs.Seek(item.Offset, SeekOrigin.Begin);
                      fs.Write(item.Content, 0, item.Content.Length);
                    }
                    if (destFile != lastFile)
                    {
                      fileIndex++;
                      lastFile = destFile;
                    }
                    break;
                  default:
                    break;
                }
                //update report status
                DateTime now = DateTime.UtcNow;
                if ( now > lastSent)
                {
                  copyPercent = Convert.ToInt32( 20 * (dirCount > 0 ? (double)dirIndex / dirCount : 1) + 80 * (fileCount > 0 ? (double)fileIndex / fileCount : 1) );
                  lastSent = now + TimeSpan.FromMilliseconds(SignalRController.SIGNALR_REFRESH_INTERVAL);
                  ReportAgentStatus(item.Operation == OperationType.WriteFolder ? StatusType.CopyFolders : StatusType.CopyFiles,
                    item.Operation == OperationType.WriteFolder ? $"Copy folders in progress {dirIndex}/{dirCount} ..." : $"Copy files in progress {fileIndex}/{fileCount} ..."
                    , copyPercent, configID);
                }
              }
              else
              {
                throw new ArgumentException("Could not extract item from queue");
              }
            }

          }
        }
        catch (Exception err)
        {
          destError = err;
          forceExceptionalStop = true;
        }
      });

      //clear exception
      forceExceptionalStop = false;

      thDest.Start();
      thSource.Start();

      //wait to complete or fail
      thSource.Join();
      thDest.Join();


      myCollection = new ConcurrentQueue<CopyOperation>();


      if (srcError != null)
      {
        outEx = srcError;
        logger.Error($"srcErr: {srcError}");
        return false;
      }
      if (destError != null)
      {
        outEx = destError;
        logger.Error($"destErr: {destError}");
        return false;
      }
      if (externalError != null)
      {
        outEx = externalError;
        logger.Debug($"externalError: {destError}");
        return false;
      }


      logger.Error($"LAN Copy completed!");

      return true;
    }



    /// <summary>
    /// helper function for adding items in the copy queue
    /// </summary>
    /// <param name="myCollection"></param>
    /// <param name="queueTHRESHOLD"></param>
    /// <param name="destError"></param>
    /// <param name="coFolder"></param>
    private void FillQueue(ConcurrentQueue<CopyOperation> myCollection, int queueTHRESHOLD, Exception destError, CopyOperation coFolder)
    {
      if (destError != null)
        throw new ArgumentException("Thread Copy Destination has failed", destError);

      while (myCollection.Count > queueTHRESHOLD)
      {
        if (forceExceptionalStop)
          return;

        Thread.Sleep(100);
      }

      myCollection.Enqueue(coFolder);
    }


    /// <summary>
    /// Helper function to combine Unc paths
    /// </summary>
    /// <param name="startPath"></param>
    /// <param name="endPath"></param>
    /// <returns></returns>
    private string UncCombine(string startPath, string endPath)
    {
      while (startPath.EndsWith(@"\"))
        startPath = startPath.Substring(0, startPath.Length - 1);
      while (endPath.StartsWith(@"\"))
        endPath = endPath.Substring(1);
      return startPath + @"\" + endPath;
    }




    private void Stop(StopMessage obj)
    {
      //signal stop
      externalError = new ApplicationException("Stop processing invoked by the service");
      forceExceptionalStop = true;
      //cleanup disposable resources
      
    }

  }









  /// <summary>
  /// helper class used in quing and sync copy operation between threads
  /// </summary>
  internal class CopyOperation
  {
    /// <summary>
    /// Operation type - can be either Write Folder or Write File
    /// </summary>
    public OperationType Operation { get; internal set; }

    /// <summary>
    /// partial folder or file path
    /// </summary>
    public string Argument { get; internal set; }
    public byte[] Content { get; internal set; }
    public long Offset { get; internal set; }

  }

  /// <summary>
  /// helper enumeration for copy operation type
  /// </summary>
  internal enum OperationType
  {
    WriteFolder,
    WriteFile
  }




}
