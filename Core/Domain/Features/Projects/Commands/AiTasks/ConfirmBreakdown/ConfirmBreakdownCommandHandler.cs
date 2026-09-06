using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.AiTasks.ConfirmBreakdown
{
    public class ConfirmBreakdownCommandHandler : IRequestHandler<ConfirmBreakdownCommand, List<TaskItemResponseDto>>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public ConfirmBreakdownCommandHandler(
            ITaskItemRepository taskItemRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _taskItemRepository = taskItemRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<List<TaskItemResponseDto>> Handle(ConfirmBreakdownCommand request, CancellationToken cancellationToken)
        {
            var requester = await _userRepository.GetByIdAsync(request.RequestedByUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var createdTasks = new List<TaskItemResponseDto>();

            foreach (var item in request.Tasks)
            {
                var priority = Enum.TryParse<Core.Enums.TaskPriority>(item.Priority, out var parsedPriority)
                    ? parsedPriority
                    : Core.Enums.TaskPriority.Medium;

                var finalAssigneeId = item.SuggestedAssigneeId ?? request.RequestedByUserId;

                var assignee = await _userRepository.GetByIdAsync(finalAssigneeId) ?? requester;

                var task = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    ProjectId = request.ProjectId,
                    AssigneeId = finalAssigneeId,
                    Title = item.Title,
                    Description = item.Description,
                    Priority = priority,
                    Status = Core.Enums.TaskStatus.ToDo,
                    PullRequestUrl = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow
                };

                await _taskItemRepository.AddAsync(task);

                await _notificationService.CreateNotificationAsync(
                    userId: finalAssigneeId,
                    tenantId: task.TenantId,
                    type: Core.Enums.NotificationType.TaskAssigned,
                    title: "تم تعيين مهمة جديدة لك",
                    message: $"تم إنشاء مهمة '{task.Title}' وتعيينها لك.",
                    taskItemId: task.Id);

                createdTasks.Add(new TaskItemResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.Priority,
                    PullRequestUrl = task.PullRequestUrl,
                    LastActivityAt = task.LastActivityAt,
                    ProjectId = task.ProjectId,
                    AssigneeId = task.AssigneeId,
                    AssigneeName = assignee.Name
                });
            }

            return createdTasks;
        }
    }
}
