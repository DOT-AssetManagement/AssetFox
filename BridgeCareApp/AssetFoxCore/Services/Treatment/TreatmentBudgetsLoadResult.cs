using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Treatment
{
    public class TreatmentBudgetsLoadResult
    {
        public List<Guid> budgetIds { get; set; }

        public List<string> ValidationMessages { get; set; }
    }
}
