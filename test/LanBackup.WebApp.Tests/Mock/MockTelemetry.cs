using System;
using LanBackup.WebApp.Models.Telemetry;

namespace LanBackup.WebApp.Tests.IntegrationTests
{
  internal class MockTelemetry : ITelemetryLogger
  {
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