using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.DataSources;
using AssetFox.Validation;

namespace AssetFox.Core.UnitTestsCore
{
    public static class TreatmentCostDtos
    {
        public static TreatmentCostDTO Dto(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            return new TreatmentCostDTO
            {
                Id = resolveId,
                Equation = new EquationDTO(),
            };
        }

        public static TreatmentCostDTO WithEquationAndCriterionLibrary(
            Guid? id = null,
            Guid? equationId = null,
            Guid? criterionLibraryId = null,
            string equation = null,
            string mergedCriteriaExpression = null)
        {
            var resolveMergedCriteriaExpression = mergedCriteriaExpression ?? "mergedCriteriaExpression";
            var resolveId = id ?? Guid.NewGuid();
            EquationDTO equationDto;
            if (equation == null)
            {
                equationDto = EquationDtos.AgePlus1(equationId);
            } else
            {
                var resolveEquationId = equationId ?? Guid.NewGuid();
                equationDto = EquationDtos.WithExpression(resolveEquationId, equation);
            }
            var criterionLibrary = CriterionLibraryDtos.Dto(criterionLibraryId, resolveMergedCriteriaExpression);
            var cost = new TreatmentCostDTO
            {
                Id = resolveId,
                Equation = equationDto,
                CriterionLibrary = criterionLibrary,
            };
            return cost;
        }
    }
}
