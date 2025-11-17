using AutoMapper;
using DTO;
using Smart_Recipe_Generator.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<AddCategoryDto, Category>();
        CreateMap<Product, ProductDto>();
        CreateMap<AddProductDto, Product>();
    }
}
