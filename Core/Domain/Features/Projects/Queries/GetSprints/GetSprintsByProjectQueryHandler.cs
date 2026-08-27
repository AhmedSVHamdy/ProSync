using AutoMapper;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetProjectById
{
    public class GetSprintsByProjectQueryHandler : IRequestHandler<GetSprintsByProjectQuery, List<SprintResponseDto>>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IMapper _mapper;

        public GetSprintsByProjectQueryHandler(ISprintRepository sprintRepository, IMapper mapper)
        {
            _sprintRepository = sprintRepository;
            _mapper = mapper;
        }

        public async Task<List<SprintResponseDto>> Handle(GetSprintsByProjectQuery request, CancellationToken cancellationToken)
        {
            var sprints = await _sprintRepository.GetByProjectIdAsync(request.ProjectId);
            return _mapper.Map<List<SprintResponseDto>>(sprints);
        }
    }
}
