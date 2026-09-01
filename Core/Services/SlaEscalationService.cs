using Core.Domain.RepositoryContracts;
using Core.Enums;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class SlaEscalationService : ISlaEscalationService
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ITaskNotifier _taskNotifier;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        private const int OverdueThresholdHours = 48;   // فاكر مثال المشروع الأصلي؟ 48 ساعة زي ما اتفقنا

        public SlaEscalationService(
            ITaskItemRepository taskItemRepository,
            ITaskNotifier taskNotifier,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _taskItemRepository = taskItemRepository;
            _taskNotifier = taskNotifier;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        public async Task CheckForOverdueCriticalTasksAsync()
        {
            var overdueTasks = await _taskItemRepository.GetOverdueCriticalTasksAsync(OverdueThresholdHours);

            foreach (var task in overdueTasks)
            {
                var managers = await _userRepository.GetManagersByTenantIdForSystemAsync(task.TenantId);   // ← الجديدة

                foreach (var manager in managers)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId: manager.Id,
                        tenantId: task.TenantId,
                        type: NotificationType.EscalationAlert,
                        title: "تنبيه: مهمة حرجة متأخرة",
                        message: $"المهمة '{task.Title}' (أولوية حرجة) لم يتم تحديثها منذ أكثر من {OverdueThresholdHours} ساعة.",
                        taskItemId: task.Id);
                }

                await _taskNotifier.NotifyTaskEscalatedAsync(task.ProjectId, task.Id, task.Title);
            }
        }
    }
}   
