using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles: Profile
{
    public AutoMapperProfiles()
    {

        CreateMap<Product, ProductDto>();
        CreateMap<ProductCategory, ProductCategoryDto>();
        
        CreateMap<Client, ClientDto>();

    }
}