using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFoxCoreTests.Tests
{
    public static class CalculatedAttributeLibraryDtos
    {
        public static CalculatedAttributeLibraryDTO Empty(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = new CalculatedAttributeLibraryDTO
            {
                Id = resolveId,
            };
            return dto;
        }
    }
}
