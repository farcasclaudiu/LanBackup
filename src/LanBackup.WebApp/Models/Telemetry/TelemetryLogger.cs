using Microsoft.ApplicationInsights;
using System;

namespace LanBackup.WebApp.Models.Telemetry
{
  public class TelemetryLogger : ITelemetryLogger
  {
    private TelemetryClient _telemetryClient;

    public TelemetryLogger(TelemetryClient telemetryClient)
    {
      this._telemetryClient = telemetryClient;
    }

    public void TrackEvent(string eventmsg)
    {
      this._telemetryClient.TrackEvent(eventmsg);
    }

    public void TrackException(Exception ex)
    {
      this._telemetryClient.TrackException(ex);
    }
  }

}
