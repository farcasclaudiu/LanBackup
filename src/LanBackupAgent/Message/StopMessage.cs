using TinyMessenger;

namespace LanBackupAgent.Message
{
  public class StopMessage : TinyMessageBase
  {
    public StopMessage(string msg) : base(msg)
    {
    }
  }
}