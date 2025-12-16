using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests.Benefit;
using AssetFox.Core.UnitTestsCore.Tests;

namespace AssetFox.Core.UnitTestsCore
{
    public static class AnalysisMethodDtos
    {
        public static AnalysisMethodDTO Default(Guid id, string attributeName = "attribute")
        {
            var benefit = BenefitDtos.Dto(attributeName);
            var criterionLibrary = new CriterionLibraryDTO();
            var dto = new AnalysisMethodDTO
            {
                Benefit = benefit,
                CriterionLibrary = criterionLibrary,
                Id = id,
                OptimizationStrategy = OptimizationStrategy.Benefit,
                SpendingStrategy = SpendingStrategy.NoSpending,
            };
            return dto;
        }

        public static AnalysisMethodDTO RiskScore()
        {
            var id = Guid.NewGuid();
            var criterionLibrary = new CriterionLibraryDTO();
            var dto = new AnalysisMethodDTO
            {
                Attribute = TestAttributeNames.RiskScore,
                Benefit = BenefitDtos.ConditionIndex(),
                CriterionLibrary = criterionLibrary,
                Id = id,
                OptimizationStrategy = OptimizationStrategy.Benefit,
                SpendingStrategy = SpendingStrategy.AsBudgetPermits,
            };
            return dto;
        }

    }
}
