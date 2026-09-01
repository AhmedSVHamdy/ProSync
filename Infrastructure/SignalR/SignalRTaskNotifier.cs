using Core.DTO;
using Core.ServiceContracts;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.SignalR
{
    public class SignalRTaskNotifier : ITaskNotifier
    {
        private readonly IHubContext<KanbanHub> _hubContext;

        public SignalRTaskNotifier(IHubContext<KanbanHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTaskStatusChangedAsync(Guid projectId, TaskItemResponseDto task)
        {
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("TaskStatusChanged", task);
        }

        public async Task NotifyTaskAssignedAsync(Guid projectId, TaskItemResponseDto task)
        {
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("TaskAssigned", task);
        }

        // في SignalRTaskNotifier (فاكرها في WebApi/Services؟)
        public async Task NotifyTaskEscalatedAsync(Guid projectId, Guid taskId, string taskTitle)
        {
            await _hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("TaskEscalated", new { taskId, taskTitle });
        }
    }
}
