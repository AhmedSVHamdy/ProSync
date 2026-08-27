using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface ITaskNotifier
    {
        Task NotifyTaskStatusChangedAsync(Guid projectId, TaskItemResponseDto task);
        Task NotifyTaskAssignedAsync(Guid projectId, TaskItemResponseDto task);
    }
}
