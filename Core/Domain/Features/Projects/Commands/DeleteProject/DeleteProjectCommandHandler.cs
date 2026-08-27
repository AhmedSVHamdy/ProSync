using Core.Domain.RepositoryContracts;
using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.DeleteProject
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;

        public DeleteProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            // Soft Delete بدل Hard Delete، بنفس فلسفة الـ User
            project.Status = ProjectStatus.Archived;
            await _projectRepository.UpdateAsync(project);
        }
    }
}
