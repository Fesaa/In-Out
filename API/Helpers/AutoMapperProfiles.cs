using API.DTOs;
using API.Entities;
using AutoMapper;
using Stock = API.Entities.Stock;

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
        CreateMap<Stock, StockDto>()
            .ForMember(d => d.Name, 
                opt => 
                    opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.Description, 
                opt => 
                    opt.MapFrom(s => s.Product.Description));
        CreateMap<StockHistory, StockHistoryDto>();

    }
}