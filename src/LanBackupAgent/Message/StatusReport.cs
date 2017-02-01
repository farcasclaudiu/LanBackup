using LanBackup.Models;
using TinyMessenger;

namespace LanBackupAgent.Message
{
  public class StatusReport : TinyMessageBase
  {
    public StatusReportInfo Info { get; set; }
    public StatusReport(string sender, StatusReportInfo msg) : base(sender)
    {
      this.Info = msg;
    }
  }
}
