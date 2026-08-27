using AutoMapper;
using Core.Domain.Entities;
using Core.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Mappers
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<User, UserProfileResponseDto>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));

            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())   // مبنعملش Map مباشر للباسورد، لازم يتعمله Hash الأول في الـ Service
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())      // بيتحدد بعد ما الـ Tenant يتعمل، مش من الـ DTO
                .ForMember(dest => dest.Role, opt => opt.Ignore())          // بيتحدد بالكود (Owner) مش من اليوزر
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
