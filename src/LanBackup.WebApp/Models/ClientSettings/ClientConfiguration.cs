using Microsoft.Extensions.Configuration;

namespace LanBackup.WebApp.Models
{
  public class ClientConfiguration
  {

    public ClientSettingsData ClientSettings { get; set; }

    public ClientConfiguration(IConfigurationRoot conf)
    {
      this.ClientSettings = conf.GetSection("clientSettings").Get<ClientSettingsData>();
    }

    public static ClientConfiguration Instance { get; set; }


  }
}
