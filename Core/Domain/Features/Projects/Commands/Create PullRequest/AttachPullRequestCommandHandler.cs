using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.Create_PullRequest
{
    public class AttachPullRequestCommandHandler : IRequestHandler<AttachPullRequestCommand, TaskItemResponseDto>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskNotifier _taskNotifier;
        private readonly INotificationService _notificationService;

        public AttachPullRequestCommandHandler(
            ITaskItemRepository taskItemRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ITaskNotifier taskNotifier,
            INotificationService notificationService)
        {
            _taskItemRepository = taskItemRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _taskNotifier = taskNotifier;
            _notificationService = notificationService;
        }

        public async Task<TaskItemResponseDto> Handle(AttachPullRequestCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskItemRepository.GetByIdWithAssigneeAsync(request.Id)
                ?? throw new InvalidOperationException("المهمة غير موجودة.");

            task.PullRequestUrl = request.PullRequestUrl;
            task.Status = Core.Enums.TaskStatus.Review;
            task.LastActivityAt = DateTime.UtcNow;

            await _taskItemRepository.UpdateAsync(task);

            var responseDto = new TaskItemResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                PullRequestUrl = task.PullRequestUrl,
                LastActivityAt = task.LastActivityAt,
                ProjectId = task.ProjectId,
                SprintId = task.SprintId,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee.Name,
                Priority = task.Priority,
                Description = task.Description
            };

            await _taskNotifier.NotifyTaskStatusChangedAsync(task.ProjectId, responseDto);

            // فاكر مين المفروض ياخد الإشعار هنا؟ مش الموظف، لكن مدراء المشروع (Owner/Admin)
            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
            var teamMembers = await _userRepository.GetAllByTenantIdAsync(task.TenantId);
            var managers = teamMembers.Where(u => u.Role == UserRole.Owner.ToString() || u.Role == UserRole.Admin.ToString());

            foreach (var manager in managers)
            {
                await _notificationService.CreateNotificationAsync(
                    userId: manager.Id,
                    tenantId: task.TenantId,
                    type: NotificationType.PullRequestMerged,   // فاكرها من الـ Enum اللي عملناه بدري؟
                    title: "طلب مراجعة كود جديد",
                    message: $"قام {task.Assignee.Name} برفع Pull Request للمهمة '{task.Title}'، بانتظار المراجعة.",
                    taskItemId: task.Id);
            }

            return responseDto;
        }
    }
}
