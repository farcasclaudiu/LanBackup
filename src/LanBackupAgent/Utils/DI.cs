using SimpleInjector;

namespace LanBackupAgent
{
  public static class DI
  {
    //the DI container
    public static Container Container { get; set; } = new Container();
  }
}
