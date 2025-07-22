using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<Client, OutputClientDto>();
            CreateMap<CreateClientDto, Client>();
            CreateMap<UpdateClientDto, Client>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<Client, Client>();
        }
    }
}
