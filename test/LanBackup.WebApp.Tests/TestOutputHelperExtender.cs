using System;
using Xunit.Abstractions;

namespace LanBackup.WebApp.Tests
{
  public static class TestOutputHelperExtender
  {
    public static void Log(this ITestOutputHelper output, string msg)
    {
      output.WriteLine(msg);
      Console.WriteLine(msg);
    }
  }
}
