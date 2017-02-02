using System;

namespace LanBackup.WebApp.Models.Telemetry
{
  public interface ITelemetryLogger
  {
    void TrackException(Exception ex);
    void TrackEvent(string v);

    bool IsEnabled { get; set; }
  }
}
