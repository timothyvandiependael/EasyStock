using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class UserPermissionMappingProfile : Profile
    {
        public UserPermissionMappingProfile()
        {
            CreateMap<UserPermission, UserPermissionOverview>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<UserPermission, OutputUserPermissionDetailDto>();
            CreateMap<UserPermissionOverview, OutputUserPermissionOverviewDto>();
            CreateMap<CreateUserPermissionDto, UserPermission>();
            CreateMap<UpdateUserPermissionDto, UserPermission>();
        }
    }
}
