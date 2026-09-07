using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.ReassignTask
{
    public class ReassignTaskCommandHandler : IRequestHandler<ReassignTaskCommand, TaskItemResponseDto>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskNotifier _taskNotifier;
        private readonly INotificationService _notificationService;

        public ReassignTaskCommandHandler(
            ITaskItemRepository taskItemRepository,
            IUserRepository userRepository,
            ITaskNotifier taskNotifier,
            INotificationService notificationService)
        {
            _taskItemRepository = taskItemRepository;
            _userRepository = userRepository;
            _taskNotifier = taskNotifier;
            _notificationService = notificationService;
        }

        public async Task<TaskItemResponseDto> Handle(ReassignTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskItemRepository.GetByIdWithAssigneeAsync(request.Id)
                ?? throw new InvalidOperationException("المهمة غير موجودة.");

            var newAssignee = await _userRepository.GetByIdAsync(request.NewAssigneeId)
                ?? throw new InvalidOperationException("الموظف الجديد غير موجود.");

            // فاكر نفس التحقق الأمني اللي عملناه في CreateTaskItemCommandHandler؟ نفس المبدأ هنا
            if (newAssignee.TenantId != request.TenantId)
                throw new UnauthorizedAccessException("لا يمكن تعيين موظف من شركة أخرى.");

            var oldAssigneeId = task.AssigneeId;

            task.AssigneeId = request.NewAssigneeId;
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
                AssigneeName = newAssignee.Name
            };

            await _taskNotifier.NotifyTaskStatusChangedAsync(task.ProjectId, responseDto);

            // إشعار للموظف الجديد
            await _notificationService.CreateNotificationAsync(
                userId: request.NewAssigneeId,
                tenantId: task.TenantId,
                type: Core.Enums.NotificationType.TaskAssigned,
                title: "تم تعيين مهمة جديدة لك",
                message: $"تم تعيينك على المهمة '{task.Title}'.",
                taskItemId: task.Id);

            // إشعار للموظف القديم (لو كان مختلف)، إنه اتشال من التاسك
            if (oldAssigneeId != request.NewAssigneeId)
            {
                await _notificationService.CreateNotificationAsync(
                    userId: oldAssigneeId,
                    tenantId: task.TenantId,
                    type: Core.Enums.NotificationType.TaskStatusChanged,
                    title: "تم إعادة تعيين مهمتك",
                    message: $"تم نقل المهمة '{task.Title}' إلى موظف آخر.",
                    taskItemId: task.Id);
            }

            return responseDto;
        }
    }
}
