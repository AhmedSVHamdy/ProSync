using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.ServiceContracts;
using Core.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Services
{
    public class GitHubWebhookServiceTests
    {
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;
        private readonly Mock<ITaskNotifier> _taskNotifierMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly GitHubWebhookService _webhookService;

        public GitHubWebhookServiceTests()
        {
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();
            _taskNotifierMock = new Mock<ITaskNotifier>();
            _notificationServiceMock = new Mock<INotificationService>();

            _webhookService = new GitHubWebhookService(
                _taskItemRepositoryMock.Object,
                _taskNotifierMock.Object,
                _notificationServiceMock.Object);
        }

        [Fact]
        public async Task HandlePullRequestEventAsync_WhenMerged_MarksTaskAsDone()
        {
            var prUrl = "https://github.com/test/repo/pull/1";
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                Title = "تصميم قاعدة البيانات",
                Status = Core.Enums.TaskStatus.Review,
                PullRequestUrl = prUrl
            };

            var payload = $$"""
                {
                    "action": "closed",
                    "pull_request": {
                        "html_url": "{{prUrl}}",
                        "merged": true
                    }
                }
                """;

            _taskItemRepositoryMock.Setup(r => r.GetByPullRequestUrlAsync(prUrl)).ReturnsAsync(task);

            await _webhookService.HandlePullRequestEventAsync(payload);

            task.Status.Should().Be(Core.Enums.TaskStatus.Done);

            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);

            _notificationServiceMock.Verify(
                n => n.CreateNotificationAsync(
                    task.AssigneeId, task.TenantId, Core.Enums.NotificationType.PullRequestMerged,
                    It.IsAny<string>(), It.IsAny<string>(), task.Id),
                Times.Once);
        }

        [Fact]
        public async Task HandlePullRequestEventAsync_WhenClosedWithoutMerge_RevertsTaskToToDo()
        {
            // فاكر ده بالظبط الـ Gap اللي إنت لقيته وصلحناه؟ ده الـ Test اللي بيثبته
            var prUrl = "https://github.com/test/repo/pull/2";
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                Title = "تطوير الواجهة",
                Status = Core.Enums.TaskStatus.Review,
                PullRequestUrl = prUrl
            };

            var payload = $$"""
                {
                    "action": "closed",
                    "pull_request": {
                        "html_url": "{{prUrl}}",
                        "merged": false
                    }
                }
                """;

            _taskItemRepositoryMock.Setup(r => r.GetByPullRequestUrlAsync(prUrl)).ReturnsAsync(task);

            await _webhookService.HandlePullRequestEventAsync(payload);

            task.Status.Should().Be(Core.Enums.TaskStatus.ToDo);
            task.PullRequestUrl.Should().BeEmpty();

            _notificationServiceMock.Verify(
                n => n.CreateNotificationAsync(
                    task.AssigneeId, task.TenantId, Core.Enums.NotificationType.TaskStatusChanged,
                    It.IsAny<string>(), It.IsAny<string>(), task.Id),
                Times.Once);
        }

        [Fact]
        public async Task HandlePullRequestEventAsync_WhenActionIsNotClosed_DoesNothing()
        {
            // فاكر الشرط الأول في الكود؟ "if (action != closed) return;" — ده بيتأكد منه
            var payload = """
                {
                    "action": "opened",
                    "pull_request": {
                        "html_url": "https://github.com/test/repo/pull/3",
                        "merged": false
                    }
                }
                """;

            await _webhookService.HandlePullRequestEventAsync(payload);

            _taskItemRepositoryMock.Verify(r => r.GetByPullRequestUrlAsync(It.IsAny<string>()), Times.Never);
            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task HandlePullRequestEventAsync_WhenTaskNotFound_DoesNothingSilently()
        {
            // فاكر ليه بنتأكد من ده؟ لو الـ PR ده مش مرتبط بأي تاسك في نظامنا، مفروض الكود يتجاهله بهدوء من غير Exception
            var prUrl = "https://github.com/unrelated/repo/pull/99";
            var payload = $$"""
                {
                    "action": "closed",
                    "pull_request": {
                        "html_url": "{{prUrl}}",
                        "merged": true
                    }
                }
                """;

            _taskItemRepositoryMock.Setup(r => r.GetByPullRequestUrlAsync(prUrl)).ReturnsAsync((TaskItem?)null);

            var act = async () => await _webhookService.HandlePullRequestEventAsync(payload);

            await act.Should().NotThrowAsync();
            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task HandlePullRequestReviewEventAsync_WhenChangesRequested_SendsNotificationOnly()
        {
            var prUrl = "https://github.com/test/repo/pull/4";
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                Title = "دمج Redis Cache",
                Status = Core.Enums.TaskStatus.Review,
                PullRequestUrl = prUrl
            };

            var payload = $$"""
                {
                    "review": {
                        "state": "changes_requested",
                        "body": "محتاج إضافة Null Check هنا"
                    },
                    "pull_request": {
                        "html_url": "{{prUrl}}"
                    }
                }
                """;

            _taskItemRepositoryMock.Setup(r => r.GetByPullRequestUrlAsync(prUrl)).ReturnsAsync(task);

            await _webhookService.HandlePullRequestReviewEventAsync(payload);

            // فاكر الفرق المهم؟ الحالة تفضل Review، مش بترجع ToDo زي الرفض النهائي
            task.Status.Should().Be(Core.Enums.TaskStatus.Review);

            _notificationServiceMock.Verify(
                n => n.CreateNotificationAsync(
                    task.AssigneeId, task.TenantId, Core.Enums.NotificationType.TaskStatusChanged,
                    It.IsAny<string>(), It.Is<string>(msg => msg.Contains("محتاج إضافة Null Check هنا")), task.Id),
                Times.Once);

            _taskItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        }

        [Fact]
        public async Task HandlePullRequestReviewEventAsync_WhenApproved_DoesNothing()
        {
            var payload = """
                {
                    "review": {
                        "state": "approved",
                        "body": ""
                    },
                    "pull_request": {
                        "html_url": "https://github.com/test/repo/pull/5"
                    }
                }
                """;

            await _webhookService.HandlePullRequestReviewEventAsync(payload);

            _taskItemRepositoryMock.Verify(r => r.GetByPullRequestUrlAsync(It.IsAny<string>()), Times.Never);
            _notificationServiceMock.Verify(
                n => n.CreateNotificationAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Core.Enums.NotificationType>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Never);
        }
    }
}
