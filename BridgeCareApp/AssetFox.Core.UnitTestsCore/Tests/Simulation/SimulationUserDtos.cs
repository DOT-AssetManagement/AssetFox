using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationUserDtos
    {
        public static SimulationUserDTO Dto(Guid? userId = null, string username = "username")
        {
            var resolveId = userId ?? Guid.NewGuid();
            var dto = new SimulationUserDTO
            {
                CanModify = true,
                IsOwner = true,
                UserId = resolveId,
                Username = username,
            };
            return dto;
        }
    }
}
