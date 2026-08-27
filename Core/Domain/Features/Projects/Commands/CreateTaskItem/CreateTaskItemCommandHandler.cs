using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CreateTaskItem
{
    public class CreateTaskItemCommandHandler : IRequestHandler<CreateTaskItemCommand, TaskItemResponseDto>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public CreateTaskItemCommandHandler(
            ITaskItemRepository taskItemRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository)
        {
            _taskItemRepository = taskItemRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<TaskItemResponseDto> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
        {
            // فاكر ليه بنتحقق من كل Foreign Key هنا؟ (شرحتها بعد الكود)
            var project = await _projectRepository.GetByIdAsync(request.ProjectId)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            var assignee = await _userRepository.GetByIdAsync(request.AssigneeId)
                ?? throw new InvalidOperationException("الموظف المعيّن غير موجود.");

            if (assignee.TenantId != request.TenantId)
                throw new UnauthorizedAccessException("لا يمكن تعيين موظف من شركة أخرى.");

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ProjectId = request.ProjectId,
                SprintId = request.SprintId,
                AssigneeId = request.AssigneeId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = Core.Enums.TaskStatus.ToDo,
                PullRequestUrl = string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            await _taskItemRepository.AddAsync(task);

            return new TaskItemResponseDto
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
                AssigneeName = assignee.Name
            };
        }
    }
}
