using Core.Domain.Entities;
using Core.Domain.Features.Projects.Commands.UpdateTaskStatus;
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
    public class UpdateTaskStatusCommandHandlerTests
    {
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;
        private readonly Mock<ISprintRepository> _sprintRepositoryMock;
        private readonly UpdateTaskStatusCommandHandler _handler;
        private readonly Mock<ITaskNotifier> _taskNotifierMock;
        private readonly Mock<INotificationService> _notificationServiceMock;

        public UpdateTaskStatusCommandHandlerTests()
        {
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();
            _sprintRepositoryMock = new Mock<ISprintRepository>();
            _taskNotifierMock = new Mock<ITaskNotifier>();
            _notificationServiceMock = new Mock<INotificationService>();

            _handler = new UpdateTaskStatusCommandHandler(
                _taskItemRepositoryMock.Object,
                _sprintRepositoryMock.Object,
                _taskNotifierMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenAssigneeUpdatesOwnTask_UpdatesStatusSuccessfully()
        {
            var userId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                AssigneeId = userId,
                Assignee = new User { Id = userId, Name = "أحمد" },
                Status = Core.Enums.TaskStatus.ToDo,
                SprintId = null
            };

            var command = new UpdateTaskStatusCommand
            {
                Id = task.Id,
                NewStatus = Core.Enums.TaskStatus.InProgress,
                CurrentUserId = userId,
                CurrentUserRole = UserRole.Member.ToString()
            };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(Core.Enums.TaskStatus.InProgress);
            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNonAssigneeMemberTriesToUpdate_ThrowsUnauthorizedAccessException()
        {
            // فاكر التحقق اللي أضفناه؟ Member مش هو الـ Assignee، ومش Admin/Owner
            var assigneeId = Guid.NewGuid();
            var otherMemberId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                AssigneeId = assigneeId,
                Assignee = new User { Id = assigneeId, Name = "أحمد" },
                Status = Core.Enums.TaskStatus.ToDo
            };

            var command = new UpdateTaskStatusCommand
            {
                Id = task.Id,
                NewStatus = Core.Enums.TaskStatus.InProgress,
                CurrentUserId = otherMemberId,   // موظف تاني، مش صاحب التاسك
                CurrentUserRole = UserRole.Member.ToString()
            };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("لا يمكنك تعديل مهمة غير معيّنة لك.");

            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenAdminUpdatesAnyTask_UpdatesSuccessfully()
        {
            // فاكر إن Admin/Owner المفروض يقدروا يعدّلوا أي تاسك، حتى لو مش معيّنة عليهم
            var assigneeId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                AssigneeId = assigneeId,
                Assignee = new User { Id = assigneeId, Name = "أحمد" },
                Status = Core.Enums.TaskStatus.ToDo
            };

            var command = new UpdateTaskStatusCommand
            {
                Id = task.Id,
                NewStatus = Core.Enums.TaskStatus.Done,
                CurrentUserId = adminId,
                CurrentUserRole = UserRole.Admin.ToString()
            };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(Core.Enums.TaskStatus.Done);
        }

        [Fact]
        public async Task Handle_WhenSprintIsClosed_ThrowsInvalidOperationException()
        {
            // فاكر ده أهم Business Rule في المشروع كله؟ "تأكد أن السيستم يرفض نقل مهمة إلى Sprint منتهي"
            var userId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                AssigneeId = userId,
                Assignee = new User { Id = userId, Name = "أحمد" },
                Status = Core.Enums.TaskStatus.InProgress,
                SprintId = sprintId
            };

            var closedSprint = new Sprint { Id = sprintId, IsClosed = true };

            var command = new UpdateTaskStatusCommand
            {
                Id = task.Id,
                NewStatus = Core.Enums.TaskStatus.Done,
                CurrentUserId = userId,
                CurrentUserRole = UserRole.Member.ToString()
            };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);
            _sprintRepositoryMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(closedSprint);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("لا يمكن تعديل حالة مهمة تابعة لسبرنت مقفول.");

            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSprintIsOpen_UpdatesSuccessfully()
        {
            var userId = Guid.NewGuid();
            var sprintId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                AssigneeId = userId,
                Assignee = new User { Id = userId, Name = "أحمد" },
                Status = Core.Enums.TaskStatus.InProgress,
                SprintId = sprintId
            };

            var openSprint = new Sprint { Id = sprintId, IsClosed = false };

            var command = new UpdateTaskStatusCommand
            {
                Id = task.Id,
                NewStatus = Core.Enums.TaskStatus.Done,
                CurrentUserId = userId,
                CurrentUserRole = UserRole.Member.ToString()
            };

            _taskItemRepositoryMock.Setup(r => r.GetByIdWithAssigneeAsync(task.Id)).ReturnsAsync(task);
            _sprintRepositoryMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(openSprint);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(Core.Enums.TaskStatus.Done);
            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }
    }
}
