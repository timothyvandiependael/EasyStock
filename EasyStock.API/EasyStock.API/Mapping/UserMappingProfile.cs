using EasyStock.API.Dtos;
using EasyStock.API.Models;
using AutoMapper;

namespace EasyStock.API.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, OutputUserDetailDto>();
            CreateMap<User, OutputUserOverviewDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}
