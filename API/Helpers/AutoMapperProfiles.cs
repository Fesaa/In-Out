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
        CreateMap<User, UserDto>();

        CreateMap<Delivery, DeliveryDto>()
            .ForMember(d => d.FromId,
                opt =>
                    opt.MapFrom(s => s.UserId))
            .ForMember(d => d.ClientId,
                opt =>
                    opt.MapFrom(s => s.Recipient.Id));
        CreateMap<DeliveryLine, DeliveryLineDto>();

    }
}