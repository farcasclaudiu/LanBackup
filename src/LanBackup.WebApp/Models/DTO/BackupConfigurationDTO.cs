using System.ComponentModel.DataAnnotations;

namespace LanBackup.WebApp.Models.DTO
{
  public class BackupConfigurationDTO
  {

    public BackupConfigurationDTO()
    {
    }

    public string ID { get; set; }
    public byte[] RowVersion { get; set; }
    [RegularExpression(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$")]
    [Required]
    public string ClientIP { get; set; }
    public string SrcFolder { get; set; }
    public string SrcUser { get; set; }
    public string SrcPass { get; set; }
    public string DestLanFolder { get; set; }
    public string DestUser { get; set; }
    public string DestPass { get; set; }
    public bool IsActive { get; set; }
    public string Crontab { get; set; }

  }
}
