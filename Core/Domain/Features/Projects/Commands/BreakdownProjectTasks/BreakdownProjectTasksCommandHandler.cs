using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.BreakdownProjectTasks
{
    public class BreakdownProjectTasksCommandHandler : IRequestHandler<BreakdownProjectTasksCommand, List<TaskItemResponseDto>>
    {
        private readonly IAiTaskBreakdownService _aiService;
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public BreakdownProjectTasksCommandHandler(
            IAiTaskBreakdownService aiService,
            ITaskItemRepository taskItemRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _aiService = aiService;
            _taskItemRepository = taskItemRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<List<TaskItemResponseDto>> Handle(BreakdownProjectTasksCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            var requester = await _userRepository.GetByIdAsync(request.RequestedByUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            // نجيب بيانات الفريق المتاح فعلياً
            var teamMembers = new List<User>();
            foreach (var memberId in request.TeamMemberIds)
            {
                var member = await _userRepository.GetByIdAsync(memberId);
                if (member is not null && member.TenantId == request.TenantId)
                {
                    teamMembers.Add(member);
                }
            }

            if (teamMembers.Count == 0 && request.TeamMemberIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"لم يتم العثور على أي من الموظفين المحددين ({request.TeamMemberIds.Count} تم إرسالهم)، تأكد أن الـ IDs صحيحة وتنتمي لنفس شركتك.");
            }

            var teamNamesWithSpecialty = teamMembers.Select(m => (m.Name, m.Specialty)).ToList();
            var breakdownItems = await _aiService.BreakdownDescriptionAsync(request.Description, teamNamesWithSpecialty);

            if (breakdownItems.Count == 0)
                throw new InvalidOperationException("لم يتمكن الذكاء الاصطناعي من تكسير الوصف إلى مهام.");

            var createdTasks = new List<TaskItemResponseDto>();

            foreach (var item in breakdownItems)
            {
                var priority = Enum.TryParse<Core.Enums.TaskPriority>(item.Priority, out var parsedPriority)
                    ? parsedPriority
                    : Core.Enums.TaskPriority.Medium;

                // هنا التحقق المهم: الاسم اللي الـ AI اقترحه موجود فعلاً في قايمة الفريق؟
                var suggestedMember = !string.IsNullOrWhiteSpace(item.SuggestedAssigneeName)
                    ? teamMembers.FirstOrDefault(m => m.Name == item.SuggestedAssigneeName)
                    : null;

                var finalAssigneeId = suggestedMember?.Id ?? request.RequestedByUserId;   // Fallback للمدير لو الاسم مش موجود
                var finalAssigneeName = suggestedMember?.Name ?? requester.Name;

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
                title: "تم تعيين مهمة جديدة لك (ادمن الشركه)",
                message: $"تم إنشاء مهمة '{task.Title}' وتعيينها لك .",
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
                    AssigneeName = finalAssigneeName
                });
            }

            return createdTasks;
        }
    }
}
