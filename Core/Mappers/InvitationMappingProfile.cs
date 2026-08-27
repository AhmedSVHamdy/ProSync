using AutoMapper;
using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Mappers
{
    public class InvitationMappingProfile : Profile
    {
        public InvitationMappingProfile()
        {
            CreateMap<Invitation, InvitationResponseDto>();

            CreateMap<AcceptInvitationRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())   // الإيميل بييجي من الـ Invitation نفسها، مش من كلام اليوزر
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
