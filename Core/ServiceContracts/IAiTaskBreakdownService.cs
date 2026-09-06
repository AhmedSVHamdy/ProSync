using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IAiTaskBreakdownService
    {
        Task<List<TaskBreakdownItemDto>> BreakdownDescriptionAsync(string description, List<(string Name, string? Specialty)> teamMembers);
    }
}
