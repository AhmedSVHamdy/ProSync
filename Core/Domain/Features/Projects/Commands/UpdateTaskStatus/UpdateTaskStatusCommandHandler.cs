using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskItemResponseDto>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskNotifier _taskNotifier;   // ← الإضافة الجديدة
        private readonly INotificationService _notificationService;   // هنعملها تحت

        public UpdateTaskStatusCommandHandler(
            ITaskItemRepository taskItemRepository,
            ISprintRepository sprintRepository,
            ITaskNotifier taskNotifier,
            INotificationService notificationService)
        {
            _taskItemRepository = taskItemRepository;
            _sprintRepository = sprintRepository;
            _taskNotifier = taskNotifier;
            _notificationService = notificationService;
        }

        public async Task<TaskItemResponseDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskItemRepository.GetByIdWithAssigneeAsync(request.Id)
                ?? throw new InvalidOperationException("المهمة غير موجودة.");

            var isOwnerOrAdmin = request.CurrentUserRole == UserRole.Owner.ToString()
                || request.CurrentUserRole == UserRole.Admin.ToString();

            if (task.AssigneeId != request.CurrentUserId && !isOwnerOrAdmin)
                throw new UnauthorizedAccessException("لا يمكنك تعديل مهمة غير معيّنة لك.");

            if (task.SprintId.HasValue)
            {
                var sprint = await _sprintRepository.GetByIdAsync(task.SprintId.Value);
                if (sprint is not null && sprint.IsClosed)
                    throw new InvalidOperationException("لا يمكن تعديل حالة مهمة تابعة لسبرنت مقفول.");
            }

            task.Status = request.NewStatus;
            task.LastActivityAt = DateTime.UtcNow;

            await _taskItemRepository.UpdateAsync(task);

            var responseDto = new TaskItemResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                PullRequestUrl = task.PullRequestUrl,
                LastActivityAt = task.LastActivityAt,
                ProjectId = task.ProjectId,
                SprintId = task.SprintId,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee.Name
            };

            // فاكر الترتيب: الـ Real-Time أولاً (فوري)، بعدها التخزين الدائم (Notification)
            await _taskNotifier.NotifyTaskStatusChangedAsync(task.ProjectId, responseDto);

            await _notificationService.CreateNotificationAsync(
                userId: task.AssigneeId,
                tenantId: task.TenantId,
                type: NotificationType.TaskStatusChanged,
                title: "تحديث حالة المهمة",
                message: $"تم تغيير حالة المهمة '{task.Title}' إلى {task.Status}",
                taskItemId: task.Id);

            return responseDto;
        }
    }
}
