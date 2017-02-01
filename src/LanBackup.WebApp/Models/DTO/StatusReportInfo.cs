using Newtonsoft.Json;
using System;

namespace LanBackup.Models
{
  //[Json]
  public class StatusReportInfo
  {
    [JsonProperty(PropertyName ="ip")]
    public string IP { get; set; }

    [JsonProperty(PropertyName = "configurationId")]
    public string ConfigurationId { get; set; }

    [JsonProperty(PropertyName = "statusType")]
    public StatusType StatusType { get; set; }

    [JsonProperty(PropertyName = "statusDescription")]
    public string StatusDescription { get; set; }

    [JsonProperty(PropertyName = "statusPercent")]
    public int StatusPercent { get; set; } = -1;

    [JsonProperty(PropertyName = "statusDateTime")]
    public DateTime StatusDateTime { get; set; }
  }


  public enum StatusType
  {
    Idle,
    Starting,
    CopyFolders,
    CopyFiles,
    Error
  }

}
