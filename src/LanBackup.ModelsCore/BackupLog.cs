using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LanBackup.ModelsCore
{
  public class BackupLog
  {

    public BackupLog()
    { }


    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }


    [Timestamp]
    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; }

    
    [RegularExpression(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$")]
    [Display(Name = "Client IP")]
    public string ClientIP { get; set; }

    [Display(Name = "Configuration IP")]
    public string ConfigurationID { get; set; }

    [Display(Name = "Description")]
    public string Description { get; set; }

    [Display(Name = "LogError")]
    public string LogError { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }


    [Display(Name = "DateTime")]
    public DateTime DateTime { get; set; }


  }

}
