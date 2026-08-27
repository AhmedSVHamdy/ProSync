using AutoMapper;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetProjects
{
    public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ProjectResponseDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            return _mapper.Map<ProjectResponseDto>(project);
        }
    }
}
