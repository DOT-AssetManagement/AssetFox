using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services
{
    internal class TreatmentBudgetCloner
    {
        internal static TreatmentBudgetDTO Clone(TreatmentBudgetDTO treatmentBudget)
        {
            var clone = new TreatmentBudgetDTO
            {
                Name = treatmentBudget.Name,
            };
            return clone;
        }

    }
}
