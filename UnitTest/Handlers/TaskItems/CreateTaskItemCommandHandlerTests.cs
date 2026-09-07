using Core.Domain.Entities;
using Core.Domain.Features.Projects.Commands.CreateTaskItem;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Handlers.TaskItems
{
    public class CreateTaskItemCommandHandlerTests
    {
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly CreateTaskItemCommandHandler _handler;

        public CreateTaskItemCommandHandlerTests()
        {
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _handler = new CreateTaskItemCommandHandler(
                _taskItemRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidData_CreatesTaskSuccessfully()
        {
            var tenantId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();

            var command = new CreateTaskItemCommand
            {
                Title = "تصميم قاعدة البيانات",
                Description = "وصف تجريبي",
                Priority = TaskPriority.High,
                ProjectId = projectId,
                AssigneeId = assigneeId,
                TenantId = tenantId
            };

            var project = new Project { Id = projectId, TenantId = tenantId, Name = "مشروع تجريبي" };
            var assignee = new User { Id = assigneeId, TenantId = tenantId, Name = "أحمد" };

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assignee);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Title.Should().Be(command.Title);
            result.Status.Should().Be(Core.Enums.TaskStatus.ToDo);
            result.AssigneeName.Should().Be("أحمد");

            _taskItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistingProject_ThrowsInvalidOperationException()
        {
            var command = new CreateTaskItemCommand
            {
                Title = "تصميم قاعدة البيانات",
                ProjectId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                TenantId = Guid.NewGuid()
            };

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(command.ProjectId)).ReturnsAsync((Project?)null);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("المشروع غير موجود.");

            _taskItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithAssigneeFromDifferentTenant_ThrowsUnauthorizedAccessException()
        {
            var tenantId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();

            var command = new CreateTaskItemCommand
            {
                Title = "تصميم قاعدة البيانات",
                ProjectId = projectId,
                AssigneeId = assigneeId,
                TenantId = tenantId
            };

            var project = new Project { Id = projectId, TenantId = tenantId };
            var assigneeFromOtherTenant = new User { Id = assigneeId, TenantId = Guid.NewGuid() };   // شركة مختلفة تماماً

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(assigneeId)).ReturnsAsync(assigneeFromOtherTenant);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("لا يمكن تعيين موظف من شركة أخرى.");

            _taskItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
        }
    }
}
