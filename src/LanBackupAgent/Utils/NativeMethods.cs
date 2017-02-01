using System;
using System.Runtime.InteropServices;

namespace LanBackupAgent.Utils
{
  public class NativeMethods
  {

    [DllImport("advapi32.DLL", SetLastError = true, CharSet=CharSet.Unicode)]
    private static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

  }
}
