using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Core.Services
{
    public class GitHubWebhookService : IGitHubWebhookService
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ITaskNotifier _taskNotifier;
        private readonly INotificationService _notificationService;

        public GitHubWebhookService(
            ITaskItemRepository taskItemRepository,
            ITaskNotifier taskNotifier,
            INotificationService notificationService)
        {
            _taskItemRepository = taskItemRepository;
            _taskNotifier = taskNotifier;
            _notificationService = notificationService;
        }

        public async Task HandlePullRequestEventAsync(string rawPayload)
        {
            using var doc = JsonDocument.Parse(rawPayload);
            var root = doc.RootElement;

            var action = root.GetProperty("action").GetString();

            if (action != "closed")
                return;

            var isMerged = root.GetProperty("pull_request").GetProperty("merged").GetBoolean();
            var prUrl = root.GetProperty("pull_request").GetProperty("html_url").GetString();

            if (string.IsNullOrEmpty(prUrl))
                return;

            var task = await _taskItemRepository.GetByPullRequestUrlAsync(prUrl);
            if (task is null)
                return;

            if (isMerged)
            {
                // المسار الموجود بالفعل: قبول
                await MarkTaskAsCompletedAsync(task);
            }
            else
            {
                // المسار الجديد: رفض (اتقفل من غير Merge)
                await RejectTaskAsync(task);
            }
        }

        private async Task MarkTaskAsCompletedAsync(TaskItem task)
        {
            task.Status = Core.Enums.TaskStatus.Done;
            task.LastActivityAt = DateTime.UtcNow;
            await _taskItemRepository.UpdateAsync(task);

            var dto = MapToDto(task);
            await _taskNotifier.NotifyTaskStatusChangedAsync(task.ProjectId, dto);

            await _notificationService.CreateNotificationAsync(
                userId: task.AssigneeId,
                tenantId: task.TenantId,
                type: Core.Enums.NotificationType.PullRequestMerged,
                title: "تم إغلاق المهمة تلقائياً",
                message: $"تم دمج Pull Request الخاص بالمهمة '{task.Title}' وتم تحديث حالتها إلى مكتملة.",
                taskItemId: task.Id);
        }

        private async Task RejectTaskAsync(TaskItem task)
        {
            // فاكر السؤال بتاعك؟ "ترجع للموظف يقوله فيه تعديلات" — ده بالظبط هنا
            task.Status = Core.Enums.TaskStatus.ToDo;   // ترجع لأول القايمة، مش Review ولا Done
            task.PullRequestUrl = string.Empty;   // نفضّي اللينك القديم، عشان يحط واحد جديد بعد التعديل
            task.LastActivityAt = DateTime.UtcNow;
            await _taskItemRepository.UpdateAsync(task);

            var dto = MapToDto(task);
            await _taskNotifier.NotifyTaskStatusChangedAsync(task.ProjectId, dto);

            await _notificationService.CreateNotificationAsync(
                userId: task.AssigneeId,   // للموظف نفسه، مش للمدير
                tenantId: task.TenantId,
                type: Core.Enums.NotificationType.TaskStatusChanged,
                title: "مطلوب تعديلات على مهمتك",
                message: $"تم رفض Pull Request الخاص بالمهمة '{task.Title}'. يرجى مراجعة الملاحظات وإعادة الرفع.",
                taskItemId: task.Id);
        }

        private TaskItemResponseDto MapToDto(TaskItem task)
        {
            return new TaskItemResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                PullRequestUrl = task.PullRequestUrl,
                LastActivityAt = task.LastActivityAt,
                ProjectId = task.ProjectId,
                SprintId = task.SprintId,
                AssigneeId = task.AssigneeId
            };
        }
        public async Task HandlePullRequestReviewEventAsync(string rawPayload)
        {
            using var doc = JsonDocument.Parse(rawPayload);
            var root = doc.RootElement;

            var reviewState = root.GetProperty("review").GetProperty("state").GetString();
            // القيم الممكنة: "approved", "changes_requested", "commented"

            if (reviewState != "changes_requested")
                return;   // بس لو المراجع طلب تعديلات صراحة

            var prUrl = root.GetProperty("pull_request").GetProperty("html_url").GetString();
            if (string.IsNullOrEmpty(prUrl))
                return;

            var task = await _taskItemRepository.GetByPullRequestUrlAsync(prUrl);
            if (task is null)
                return;

            var reviewComment = root.GetProperty("review").GetProperty("body").GetString() ?? "لا توجد ملاحظات مكتوبة.";

            await _notificationService.CreateNotificationAsync(
                userId: task.AssigneeId,
                tenantId: task.TenantId,
                type: Core.Enums.NotificationType.TaskStatusChanged,
                title: "طلب تعديلات على الكود",
                message: $"المراجع طلب تعديلات على مهمتك '{task.Title}': {reviewComment}",
                taskItemId: task.Id);

            // ملحوظة: هنا التاسك ممكن تفضل في Review (لسه مش مرفوضة رسمياً، بس فيه ملاحظات)
            // القرار النهائي (رفض كامل أو قبول) بييجي من حدث pull_request نفسه لما الـ PR يتقفل فعلياً
        }
    }
}
