using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile() 
        {
            CreateMap<Category, OutputCategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
        }
    }
}
