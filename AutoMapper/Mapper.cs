using AutoMapper;
using EntraGraphAPI.Dto;
using EntraGraphAPI.Models;

namespace EntraGraphAPI.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ReceiveCustomAttributes, UsersAttributes>();
            CreateMap<RecieveUsers, Users>();
            
            // Mapping for LogAttribute and LogAttributeDTO
            CreateMap<LogAttributeDTO, LogAttribute>();

            // Mapping for nested DeviceDetail and DeviceDetailDTO
            CreateMap<DeviceDetailDTO, DeviceDetail>();

            // Mapping for nested Location and LocationDTO
            CreateMap<LocationDTO, Location>();

        }
    }
}
