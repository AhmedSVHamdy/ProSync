using AutoMapper;
using Core.Domain.Entities;
using Core.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Mappers
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<User, UserProfileResponseDto>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));
        }
    }

}