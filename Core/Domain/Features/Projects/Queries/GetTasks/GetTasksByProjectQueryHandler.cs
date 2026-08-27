using AutoMapper;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetTasks
{
    public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<TaskItemResponseDto>>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IMapper _mapper;

        public GetTasksByProjectQueryHandler(ITaskItemRepository taskItemRepository, IMapper mapper)
        {
            _taskItemRepository = taskItemRepository;
            _mapper = mapper;
        }

        public async Task<List<TaskItemResponseDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _taskItemRepository.GetByProjectIdWithAssigneeAsync(request.ProjectId);

            return _mapper.Map<List<TaskItemResponseDto>>(tasks);
        }
    }
}
