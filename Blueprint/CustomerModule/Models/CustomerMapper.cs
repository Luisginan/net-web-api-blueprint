using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Blueprint.CustomerModule.DTO;

namespace Blueprint.CustomerModule.Models;
[ExcludeFromCodeCoverage]
public class CustomerMapper: Profile
{
    
    public CustomerMapper()
    {
        CreateMap<Customer, CustomerResponse>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email2));
        CreateMap<Customer, CustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email2));
        CreateMap<CustomerResponse, Customer>()
            .ForMember(dest => dest.Email2, opt => opt.MapFrom(src => src.Email));
        CreateMap<CustomerRequest, Customer>()
            .ForMember(dest => dest.Email2, opt => opt.MapFrom(src => src.Email));
    }
}