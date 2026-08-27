using AutoMapper;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetAllProjects
{
    public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectResponseDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;   // هنا AutoMapper مناسبة، لأنها List كاملة بدون منطق إضافي

        public GetAllProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<List<ProjectResponseDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var projects = await _projectRepository.GetAllByTenantIdAsync(request.TenantId);
            return _mapper.Map<List<ProjectResponseDto>>(projects);
        }
    }
}
