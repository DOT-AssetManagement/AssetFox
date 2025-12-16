using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests;

namespace AssetFoxCoreTests.Tests
{
    public static class CalculatedAttributeEquationCriteriaPairDtos
    {
        public static CalculatedAttributeEquationCriteriaPairDTO New(Guid? criterionLibraryId = null, Guid? equationId = null)
        {
            var dto = new CalculatedAttributeEquationCriteriaPairDTO
            {
                Id = Guid.NewGuid(),
                CriteriaLibrary = CriterionLibraryDtos.Dto(criterionLibraryId),
                Equation = EquationDtos.AgePlus1(equationId),
            };
            return dto;
        }
    }
}
