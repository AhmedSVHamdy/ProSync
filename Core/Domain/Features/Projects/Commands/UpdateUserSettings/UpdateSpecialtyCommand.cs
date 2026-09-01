using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateUserSettings
{
    public class UpdateSpecialtyCommand
    {
        public string Specialty { get; set; } = string.Empty;
    }
}
