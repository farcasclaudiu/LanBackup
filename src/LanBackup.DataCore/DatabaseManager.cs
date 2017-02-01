using System.Collections.Generic;
using System.Linq;
using LanBackup.ModelsCore;

namespace LanBackup.DataCore
{
  /// <summary>
  /// Database manager class (repository)
  /// </summary>
  public class DatabaseManager
  {

    private BackupsContext context;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="mcontext"></param>
    public DatabaseManager(BackupsContext mcontext)
    {
      context = mcontext;
    }

    /// <summary>
    /// create a backup
    /// </summary>
    /// <param name="backup"></param>
    /// <returns>the new backup ID</returns>
    public string CreateBackupConfig(BackupConfiguration backup)
    {
      context.Backups.Add(backup);
      context.SaveChanges();
      return backup.ID;
    }

    /// <summary>
    /// retrieve all backups
    /// </summary>
    /// <returns></returns>
    public IEnumerable<BackupConfiguration> GetBackupConfigs()
    {
      var list = context.Backups.ToList();
      return list;
    }

    /// <summary>
    /// gets the backup by ID
    /// </summary>
    /// <param name="id">backup ID</param>
    /// <returns>backup</returns>
    public BackupConfiguration GetBackupConfig(string id)
    {
      return context.Backups.FirstOrDefault(b => b.ID == id);
    }

    public IEnumerable<BackupConfiguration> GetBackupConfigByClient(string clientid)
    {
      return context.Backups.Where(b => b.ClientIP == clientid);
    }

    /// <summary>
    /// Update backup configuration
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public int UpdateBackupConfig(BackupConfiguration entity)
    {
      context.Backups.Update(entity);
      return context.SaveChanges();
    }

    /// <summary>
    /// delete backup configuration
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int DeleteBackupConfig(string id)
    {
      BackupConfiguration entity = context.Backups.Find(id);
      context.Backups.Remove(entity);
      return context.SaveChanges();
    }
  }
}
