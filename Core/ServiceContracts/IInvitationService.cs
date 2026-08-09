using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IInvitationService
    {
        Task InviteUserAsync(Guid invitedByUserId, InviteUserRequestDto dto);
        Task<AuthResponseDto> AcceptInvitationAsync(AcceptInvitationRequestDto dto);
    }
}
