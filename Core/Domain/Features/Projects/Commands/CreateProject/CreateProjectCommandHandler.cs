using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CreateProject
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectResponseDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                Status = ProjectStatus.Active
            };

            await _projectRepository.AddAsync(project);

            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Status = project.Status,
                TenantId = project.TenantId
            };
        }
    }
}
