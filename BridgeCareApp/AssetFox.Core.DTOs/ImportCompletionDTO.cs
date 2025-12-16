using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class ImportCompletionDTO
    {
        public Guid Id { get; set; }
        public WorkType WorkType { get; set; }
        public bool AreBudgetsOverWritten { get; set; }
    }
}
