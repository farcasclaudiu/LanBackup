using NLog;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LanBackupAgent.Utils
{
  public class Network
  {

    private string localIP = string.Empty;
    ILogger logger;

    public Network(ILogger mlogger) {
      this.logger = mlogger;
      logger.Trace("Network instance initialized");
    }

    public string GetLocalIPAddress()
    {
      if (string.IsNullOrEmpty(localIP))
      {
        UnicastIPAddressInformation mostSuitableIp = null;

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var network in networkInterfaces)
        {
          if (network.OperationalStatus != OperationalStatus.Up)
            continue;

          var properties = network.GetIPProperties();

          if (properties.GatewayAddresses.Count == 0)
            continue;

          foreach (var address in properties.UnicastAddresses)
          {
            if (address.Address.AddressFamily != AddressFamily.InterNetwork)
              continue;

            if (IPAddress.IsLoopback(address.Address))
              continue;

            if (!address.IsDnsEligible)
            {
              if (mostSuitableIp == null)
                mostSuitableIp = address;
              continue;
            }

            // The best IP is the IP got from DHCP server
            if (address.PrefixOrigin != PrefixOrigin.Dhcp)
            {
              if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                mostSuitableIp = address;
              continue;
            }

            localIP = address.Address.ToString();
          }
        }
        if (string.IsNullOrEmpty(localIP))
        {
          localIP = mostSuitableIp != null
              ? mostSuitableIp.Address.ToString()
              : string.Empty;
        }
      }
      logger.Trace($"Network instance IP: {localIP}");
      return localIP;
    }

  }
}
