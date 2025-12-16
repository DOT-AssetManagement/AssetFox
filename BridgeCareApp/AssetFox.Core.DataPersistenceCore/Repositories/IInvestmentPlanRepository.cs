using System;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IInvestmentPlanRepository
    {
        void GetSimulationInvestmentPlan(Simulation simulation);

        InvestmentPlanDTO GetInvestmentPlan(Guid simulationId);

        int[] GetInvestmentStartAndEndYears(Guid simulationId);

        void UpsertInvestmentPlan(InvestmentPlanDTO dto, Guid simulationId);
    }
}
