using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateUserSettings
{
    public class UpdateUserSettingsCommand
    {
        public bool EmailNotifications { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
