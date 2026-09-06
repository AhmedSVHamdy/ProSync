using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.AiTasks.PreviewBreakdown
{
    public class PreviewBreakdownCommandHandler : IRequestHandler<PreviewBreakdownCommand, List<TaskBreakdownPreviewDto>>
    {
        private readonly IAiTaskBreakdownService _aiService;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public PreviewBreakdownCommandHandler(
            IAiTaskBreakdownService aiService,
            IProjectRepository projectRepository,
            IUserRepository userRepository)
        {
            _aiService = aiService;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<List<TaskBreakdownPreviewDto>> Handle(PreviewBreakdownCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId)
                ?? throw new InvalidOperationException("المشروع غير موجود.");

            var teamMembers = new List<User>();
            foreach (var memberId in request.TeamMemberIds)
            {
                var member = await _userRepository.GetByIdAsync(memberId);
                if (member is not null && member.TenantId == request.TenantId)
                {
                    teamMembers.Add(member);
                }
            }

            if (teamMembers.Count == 0 && request.TeamMemberIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"لم يتم العثور على أي من الموظفين المحددين، تأكد أن الـ IDs صحيحة وتنتمي لنفس شركتك.");
            }

            var teamNamesWithSpecialty = teamMembers.Select(m => (m.Name, m.Specialty)).ToList();
            var breakdownItems = await _aiService.BreakdownDescriptionAsync(request.Description, teamNamesWithSpecialty);

            var previewList = new List<TaskBreakdownPreviewDto>();

            foreach (var item in breakdownItems)
            {
                var suggestedMember = !string.IsNullOrWhiteSpace(item.SuggestedAssigneeName)
                    ? teamMembers.FirstOrDefault(m => m.Name == item.SuggestedAssigneeName)
                    : null;

                previewList.Add(new TaskBreakdownPreviewDto
                {
                    Title = item.Title,
                    Description = item.Description,
                    Priority = item.Priority,
                    SuggestedAssigneeId = suggestedMember?.Id,
                    SuggestedAssigneeName = suggestedMember?.Name
                });
            }

            return previewList;
        }
    }
}
