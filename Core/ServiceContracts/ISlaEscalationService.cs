using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface ISlaEscalationService
    {
        Task CheckForOverdueCriticalTasksAsync();
    }
}
