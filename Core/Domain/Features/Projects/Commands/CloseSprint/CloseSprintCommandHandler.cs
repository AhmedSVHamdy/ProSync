using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CloseSprint
{
    public class CloseSprintCommandHandler : IRequestHandler<CloseSprintCommand, SprintSummaryDto>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskItemRepository _taskItemRepository;

        public CloseSprintCommandHandler(ISprintRepository sprintRepository, ITaskItemRepository taskItemRepository)
        {
            _sprintRepository = sprintRepository;
             _taskItemRepository = taskItemRepository;
        }

        public async Task<SprintSummaryDto> Handle(CloseSprintCommand request, CancellationToken cancellationToken)
        {
            var sprint = await _sprintRepository.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("السبرنت غير موجود.");

            if (sprint.IsClosed)
                throw new InvalidOperationException("السبرنت مقفول بالفعل.");

            var tasks = await _taskItemRepository.GetBySprintIdAsync(sprint.Id);

            var totalTasks = tasks.Count;
            var completedTasks = tasks.Count(t => t.Status == Core.Enums.TaskStatus.Done);
            var incompleteTasks = totalTasks - completedTasks;

            sprint.IsClosed = true;
            await _sprintRepository.UpdateAsync(sprint);

            return new SprintSummaryDto
            {
                SprintId = sprint.Id,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                IncompleteTasks = incompleteTasks,
                CompletionPercentage = totalTasks == 0 ? 0 : Math.Round((double)completedTasks / totalTasks * 100, 1)
            };
        }
    }
}
