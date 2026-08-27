using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.Create_Sprint
{
    public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, SprintResponseDto>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IProjectRepository _projectRepository;

        public CreateSprintCommandHandler(ISprintRepository sprintRepository, IProjectRepository projectRepository)
        {
            _sprintRepository = sprintRepository;
            _projectRepository = projectRepository;
        }

        public async Task<SprintResponseDto> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
        {
            // تحقق مهم: المشروع ده فعلاً موجود وبتاع نفس الشركة؟
            var project = await _projectRepository.GetByIdAsync(request.ProjectId)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            var sprint = new Sprint
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ProjectId = request.ProjectId,
                Title = request.Title,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsClosed = false
            };

            await _sprintRepository.AddAsync(sprint);

            return new SprintResponseDto
            {
                Id = sprint.Id,
                Title = sprint.Title,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                IsClosed = sprint.IsClosed,
                ProjectId = sprint.ProjectId
            };
        }
    }
}
