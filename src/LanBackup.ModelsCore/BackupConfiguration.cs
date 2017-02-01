using System.ComponentModel.DataAnnotations;

namespace LanBackup.ModelsCore
{
  public class BackupConfiguration
  {
    public BackupConfiguration()
    {
    }

    [Key]
    public string ID { get; set; }


    [Timestamp]
    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; }

    
    [RegularExpression(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$")]
    [Required]
    [Display(Name = "Client IP")]
    public string ClientIP { get; set; }

    [Display(Name = "Source folder")]
    public string SrcFolder { get; set; }

    [Display(Name = "Source folder Username")]
    public string SrcUser { get; set; }

    [Display(Name = "Source folder Password")]
    public string SrcPass { get; set; }

    [Display(Name = "Destination LAN folder")]
    public string DestLanFolder { get; set; }

    [Display(Name = "Destination folder Username")]
    public string DestUser { get; set; }

    [Display(Name = "Destination folder Password")]
    public string DestPass { get; set; }

    [Display(Name = "Is Backup Active")]
    public bool IsActive { get; set; }



    //TODO - validate to be a CRONTAB expression
    //[RegularExpression(@"^[A-Z]+[a-zA-Z''-'\s]*$")]
    [Display(Name = "Crontab schedule expression")]
    public string Crontab { get; set; }

  }
}
