using System;
using LanBackup.WebApp.Models.Telemetry;

namespace LanBackup.WebApp.Tests.IntegrationTests
{
  internal class MockTelemetry : ITelemetryLogger
  {
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

    public void TrackEvent(string eventmsg)
    {
      //DO NOTHING
    }

    public void TrackException(Exception ex)
    {
      //DO NOTHING
    }
  }
}