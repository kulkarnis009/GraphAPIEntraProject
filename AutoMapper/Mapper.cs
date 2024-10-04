using AutoMapper;
using EntraGreaphAPI.Models;

namespace EntraGreaphAPI.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ReceiveCustomAttributes, CustomAttributes>();
        }
    }
}
