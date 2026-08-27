using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.SignalR
{
    [Authorize]   // محدش يقدر يتصل بالـ Hub من غير توكن صالح
    public class KanbanHub : Hub
    {
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }
    }
}
