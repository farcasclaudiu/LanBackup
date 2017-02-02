using Microsoft.ApplicationInsights;
using System;

namespace LanBackup.WebApp.Models.Telemetry
{
  public class TelemetryLogger : ITelemetryLogger
  {
    private TelemetryClient _telemetryClient;

    private bool isEnabled;
    public bool IsEnabled
    {
      get
      {
        return isEnabled;
      }

      set
      {
        isEnabled = value;
      }
    }

    public TelemetryLogger(TelemetryClient telemetryClient)
    {
      this._telemetryClient = telemetryClient;
      this.IsEnabled = telemetryClient.IsEnabled();
    }

    public void TrackEvent(string eventmsg)
    {
      if(this.IsEnabled)
        this._telemetryClient.TrackEvent(eventmsg);
    }

    public void TrackException(Exception ex)
    {
      if(this.IsEnabled)
        this._telemetryClient.TrackException(ex);
    }
  }

}
