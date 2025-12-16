using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class EquationTestSetup
    {
        public static EquationDTO Dto(string equation, Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new EquationDTO
            {
                Expression = equation,
                Id = resolveId,
            };
            return dto;
        }
    }
}
