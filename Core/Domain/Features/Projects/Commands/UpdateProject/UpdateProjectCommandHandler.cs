using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;

        public UpdateProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectResponseDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            project.Name = request.Name;
            project.Status = request.Status;

            await _projectRepository.UpdateAsync(project);

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
