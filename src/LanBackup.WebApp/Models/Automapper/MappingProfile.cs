using AutoMapper;
using LanBackup.WebApp.Models.DTO;
using LanBackup.ModelsCore;

namespace LanBackup.WebApp
{
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      // map your objects
      CreateMap<BackupConfiguration, BackupConfigurationDTO>().ReverseMap();
      CreateMap<BackupLog, BackupLogDTO>().ReverseMap();
    }
  }
}
