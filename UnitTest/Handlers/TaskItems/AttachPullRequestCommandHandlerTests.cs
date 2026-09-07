using Core.Domain.Entities;
using Core.Domain.Features.Projects.Commands.Create_PullRequest;
using Core.Domain.Features.Projects.Commands.CreateTaskItem;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using Core.ServiceContracts;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Handlers.TaskItems
{
    public class AttachPullRequestCommandHandlerTests
    {
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITaskNotifier> _taskNotifierMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly AttachPullRequestCommandHandler _handler;

        public AttachPullRequestCommandHandlerTests()
        {
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _taskNotifierMock = new Mock<ITaskNotifier>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _handler = new AttachPullRequestCommandHandler(
                _taskItemRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object,
                _taskNotifierMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidPrUrl_NotifiesManagers()
        {
            var tenantId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProjectId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                Assignee = new User { Name = "أحمد" },
                Status = Core.Enums.TaskStatus.InProgress,
                PullRequestUrl = string.Empty
            };

            var command = new AttachPullRequestCommand
            {
                Id = task.Id,
                PullRequestUrl = "https://github.com/test/repo/pull/1"
            };

            var manager = new User { Id = managerId, TenantId = tenantId, Role = UserRole.Admin.ToString(), Name = "المدير" };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);
            _userRepositoryMock.Setup(r => r.GetAllByTenantIdAsync(tenantId)).ReturnsAsync(new List<User> { manager });

            await _handler.Handle(command, CancellationToken.None);

            _notificationServiceMock.Verify(
                n => n.CreateNotificationAsync(
                    managerId, tenantId, NotificationType.PullRequestMerged,
                    It.IsAny<string>(), It.IsAny<string>(), task.Id),
                Times.Once);
        }
    }
}
