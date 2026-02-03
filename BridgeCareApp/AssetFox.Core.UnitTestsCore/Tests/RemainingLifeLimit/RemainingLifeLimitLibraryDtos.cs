using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.RemainingLifeLimit
{
    public static class RemainingLifeLimitLibraryDtos
    {
        public static RemainingLifeLimitLibraryDTO Empty(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            return new RemainingLifeLimitLibraryDTO
            {
                Id = resolveId,
                Name = "Test Name"
            };
        }
    }
}
