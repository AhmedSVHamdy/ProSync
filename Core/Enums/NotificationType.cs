using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Enums
{
    public enum NotificationType
    {
        TaskAssigned = 1,
        TaskStatusChanged = 2,
        MentionInComment = 3,
        SprintClosed = 4,
        EscalationAlert = 5,      // من الـ SLA Engine اللي اتكلمنا عليه بدري
        PullRequestMerged = 6

    }
}
