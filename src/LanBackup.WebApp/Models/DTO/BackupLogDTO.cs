using System;
using System.ComponentModel.DataAnnotations;

namespace LanBackup.WebApp.Models.DTO
{
  public class BackupLogDTO
    {

    public BackupLogDTO()
    { }

    public int ID { get; set; }
    public byte[] RowVersion { get; set; }
    [RegularExpression(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$")]
    public string ClientIP { get; set; }
    public string ConfigurationID { get; set; }
    public string Description { get; set; }
    public string LogError { get; set; }
    public string Status { get; set; }
    public DateTime DateTime { get; set; }

  }
}
